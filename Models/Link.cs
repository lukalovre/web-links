using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebLinks.Models.Interfaces;

namespace WebLinks.Models;

[Table("Links")]
public record Link : IItem
{
    [Key]
    public int ID { get; set; }

    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}