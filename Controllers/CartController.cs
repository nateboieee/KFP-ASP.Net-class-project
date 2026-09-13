using KFP.Data;
using KFP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KFP.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;

    public CartController(ApplicationDbContext context, CartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        var cart = _cartService.GetCart();
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int menuItemId, int quantity = 1, string? returnUrl = null)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == menuItemId);
        if (item is null || !item.IsAvailable)
        {
            TempData["Error"] = "Sorry, that item is not available.";
            return RedirectToAction("Index", "Menu");
        }

        _cartService.AddToCart(item, quantity < 1 ? 1 : quantity);
        TempData["Success"] = $"{item.Name} added to your bucket!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Menu");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int menuItemId, int quantity)
    {
        _cartService.UpdateQuantity(menuItemId, quantity);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int menuItemId)
    {
        _cartService.RemoveItem(menuItemId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.Clear();
        return RedirectToAction("Index");
    }
}
