namespace KFP.Models;

// Plain object stored (serialized as JSON) in the user's session.
// Keeping the cart in session rather than the database keeps checkout
// simple and lets anonymous visitors browse and add to cart before login.
public class CartItem
{
    public int MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Icon { get; set; } = "🍗";
    public int Quantity { get; set; }

    public decimal LineTotal => Price * Quantity;
}
