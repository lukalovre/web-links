using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AvaloniaApplication1.Models.Interfaces;

namespace AvaloniaApplication1.Models;

[Table("Music")]
public record Music : IItem, IExternal, IRuntime
{
    [Key]
    public int ID { get; set; }
    public string Artist { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Runtime { get; set; }
    public bool _1001 { get; set; }
    public string ExternalID { get; set; } = string.Empty;
}