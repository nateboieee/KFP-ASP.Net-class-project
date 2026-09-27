using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KFP.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000)]
    public decimal Price { get; set; }

    // Emoji shown as a fallback when no photo has been uploaded.
    [StringLength(10)]
    public string Icon { get; set; } = "🍗";

    // Relative path (e.g. "/images/menu/abc123.jpg") to an uploaded photo.
    // Null/empty means "no photo yet" - the UI falls back to the emoji Icon.
    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public bool IsSpicy { get; set; }

    public bool IsAvailable { get; set; } = true;

    public bool IsFeatured { get; set; }

    public int MenuCategoryId { get; set; }

    public MenuCategory? MenuCategory { get; set; }
}
