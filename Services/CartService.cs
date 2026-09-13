using System.Text.Json;
using KFP.Models;

namespace KFP.Services;

public class CartService
{
    private const string SessionKey = "KFP_CART";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

    public List<CartItem> GetCart()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new List<CartItem>();
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
    }

    public void AddToCart(MenuItem item, int quantity = 1)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.MenuItemId == item.Id);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                MenuItemId = item.Id,
                Name = item.Name,
                Price = item.Price,
                Icon = item.Icon,
                Quantity = quantity
            });
        }

        SaveCart(cart);
    }

    public void UpdateQuantity(int menuItemId, int quantity)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);
        if (existing is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            cart.Remove(existing);
        }
        else
        {
            existing.Quantity = quantity;
        }

        SaveCart(cart);
    }

    public void RemoveItem(int menuItemId)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.MenuItemId == menuItemId);
        SaveCart(cart);
    }

    public void Clear()
    {
        Session.Remove(SessionKey);
    }

    public int ItemCount() => GetCart().Sum(c => c.Quantity);

    public decimal Total() => GetCart().Sum(c => c.LineTotal);
}
