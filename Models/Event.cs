using System;
using System.ComponentModel.DataAnnotations;

namespace WebLinks.Models;

public record Event
{
    [Key]
    public int ID { get; set; }
    public int ItemID { get; set; }
    public DateTime Date { get; set; }
    public bool? Bookmarked { get; set; }
}
