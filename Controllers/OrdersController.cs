using KFP.Data;
using KFP.Models;
using KFP.Models.ViewModels;
using KFP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KFP.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, CartService cartService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _cartService = cartService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = _cartService.GetCart();
        if (cart.Count == 0)
        {
            TempData["Error"] = "Your bucket is empty — add something tasty first!";
            return RedirectToAction("Index", "Menu");
        }

        var user = await _userManager.GetUserAsync(User);

        var vm = new CheckoutViewModel
        {
            Items = cart,
            DeliveryAddress = user?.Address ?? string.Empty,
            PhoneNumber = user?.PhoneNumber ?? string.Empty
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel vm)
    {
        var cart = _cartService.GetCart();
        if (cart.Count == 0)
        {
            TempData["Error"] = "Your bucket is empty — add something tasty first!";
            return RedirectToAction("Index", "Menu");
        }

        vm.Items = cart;

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            DeliveryAddress = vm.DeliveryAddress,
            PhoneNumber = vm.PhoneNumber,
            PaymentMethod = vm.PaymentMethod,
            Status = OrderStatus.Placed,
            TotalAmount = cart.Sum(c => c.LineTotal),
            OrderItems = cart.Select(c => new OrderItem
            {
                MenuItemId = c.MenuItemId,
                ItemName = c.Name,
                UnitPrice = c.Price,
                Quantity = c.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _cartService.Clear();

        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await GetOwnedOrderAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    public async Task<IActionResult> History()
    {
        var userId = _userManager.GetUserId(User);
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await GetOwnedOrderAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    private async Task<Order?> GetOwnedOrderAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return null;
        }

        if (!isAdmin && order.UserId != userId)
        {
            return null;
        }

        return order;
    }
}
