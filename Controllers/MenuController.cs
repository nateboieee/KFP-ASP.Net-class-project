using KFP.Data;
using KFP.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KFP.Controllers;

public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;

    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search)
    {
        var categories = await _context.MenuCategories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var query = _context.MenuItems
            .Include(i => i.MenuCategory)
            .Where(i => i.IsAvailable)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.MenuCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search) || i.Description.Contains(search));
        }

        var items = await query.OrderBy(i => i.MenuCategory!.DisplayOrder).ThenBy(i => i.Name).ToListAsync();

        var vm = new MenuIndexViewModel
        {
            Categories = categories,
            Items = items,
            SelectedCategoryId = categoryId,
            SearchTerm = search
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.MenuItems
            .Include(i => i.MenuCategory)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }
}
