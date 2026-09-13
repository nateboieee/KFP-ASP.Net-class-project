using KFP.Data;
using KFP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KFP.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> CreateItem(MenuItem item)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(item);
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
    public async Task<IActionResult> EditItem(int id, MenuItem item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _context.MenuCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(item);
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
}
