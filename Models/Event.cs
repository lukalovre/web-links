using System;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaApplication1.Models;

public record Event
{
    [Key]
    public int ID { get; set; }
    public int ItemID { get; set; }
    public DateTime Date { get; set; }
}
