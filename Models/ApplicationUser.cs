using Microsoft.AspNetCore.Identity;

namespace KFP.Models;

// Extends the built-in Identity user with a few extra profile fields
// that a food-ordering site needs (name, delivery address, phone).
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
