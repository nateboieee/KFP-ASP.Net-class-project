using System.ComponentModel.DataAnnotations;

namespace KFP.Models;

public class MenuCategory
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    // A single emoji used as a lightweight icon for the category (no external images needed).
    [StringLength(10)]
    public string Icon { get; set; } = "🍗";

    public int DisplayOrder { get; set; }

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
