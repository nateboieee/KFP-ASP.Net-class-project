using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KFP.Models;

public enum OrderStatus
{
    Placed = 0,
    Preparing = 1,
    OutForDelivery = 2,
    Delivered = 3,
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    [Required, StringLength(300)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(30)]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
