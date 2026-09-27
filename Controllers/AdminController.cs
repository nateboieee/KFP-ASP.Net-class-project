using KFP.Data;
using KFP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KFP.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB
    private const string ImageFolderRelative = "/images/menu";

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ItemCount = await _context.MenuItems.CountAsync();
        ViewBag.OrderCount = await _context.Orders.CountAsync();
        ViewBag.UserCount = await _context.Users.CountAsync();
        return View();
    }

    // ---------------- Menu items ----------------

    public async Task<IActionResult> Items()
    {
        var items = await _context.MenuItems.Include(i => i.MenuCategory).OrderBy(i => i.Name).ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> CreateItem()
    {
        ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
        return View(new MenuItem());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> CreateItem(MenuItem item, IFormFile? imageFile)
    {
        // ImageUrl isn't posted from the form (it's file-upload driven), so make sure a
        // missing value there never fails validation.
        ModelState.Remove(nameof(MenuItem.ImageUrl));

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(item);
        }

        if (imageFile is { Length: > 0 })
        {
            var (ok, relativePath, error) = await TrySaveImageAsync(imageFile);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error!);
                ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
                return View(item);
            }

            item.ImageUrl = relativePath;
        }

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"{item.Name} added to the menu.";
        return RedirectToAction(nameof(Items));
    }

    public async Task<IActionResult> EditItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> EditItem(int id, MenuItem item, IFormFile? imageFile, bool removeImage = false)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        ModelState.Remove(nameof(MenuItem.ImageUrl));

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(item);
        }

        var existing = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        // Keep the current photo unless the admin uploads a new one or asks to remove it.
        item.ImageUrl = existing.ImageUrl;

        if (imageFile is { Length: > 0 })
        {
            var (ok, relativePath, error) = await TrySaveImageAsync(imageFile);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error!);
                ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
                return View(item);
            }

            DeleteImageFile(existing.ImageUrl);
            item.ImageUrl = relativePath;
        }
        else if (removeImage)
        {
            DeleteImageFile(existing.ImageUrl);
            item.ImageUrl = null;
        }

        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"{item.Name} updated.";
        return RedirectToAction(nameof(Items));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item is not null)
        {
            DeleteImageFile(item.ImageUrl);
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{item.Name} removed from the menu.";
        }

        return RedirectToAction(nameof(Items));
    }

    // ---------------- Orders ----------------

    public async Task<IActionResult> Orders()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is not null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Orders));
    }

    // ---------------- Image upload helpers ----------------

    private async Task<(bool Success, string? RelativePath, string? Error)> TrySaveImageAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return (false, null, "Please upload a JPG, PNG, GIF, or WEBP image.");
        }

        if (file.Length > MaxImageBytes)
        {
            return (false, null, "Image is too large. Please keep it under 5 MB.");
        }

        var folderOnDisk = Path.Combine(_env.WebRootPath, "images", "menu");
        Directory.CreateDirectory(folderOnDisk);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folderOnDisk, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return (true, $"{ImageFolderRelative}/{fileName}", null);
    }

    private void DeleteImageFile(string? relativePath)
    {
        // Only ever delete files we manage, inside wwwroot/images/menu - never trust an
        // arbitrary path here.
        if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith(ImageFolderRelative))
        {
            return;
        }

        var fileName = Path.GetFileName(relativePath);
        var fullPath = Path.Combine(_env.WebRootPath, "images", "menu", fileName);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
