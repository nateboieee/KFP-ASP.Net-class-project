using System.ComponentModel.DataAnnotations;
using KFP.Models;

namespace KFP.Models.ViewModels;

public class CheckoutViewModel
{
    public List<CartItem> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.LineTotal);

    [Required(ErrorMessage = "A delivery address is required.")]
    [StringLength(300)]
    [Display(Name = "Delivery Address")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "A phone number is required.")]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = "Cash on Delivery";
}

public class MenuIndexViewModel
{
    public List<MenuCategory> Categories { get; set; } = new();
    public List<MenuItem> Items { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public string? SearchTerm { get; set; }
}
