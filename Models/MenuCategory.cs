using System.ComponentModel.DataAnnotations;

namespace KFP.Models;

public class MenuCategory
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
