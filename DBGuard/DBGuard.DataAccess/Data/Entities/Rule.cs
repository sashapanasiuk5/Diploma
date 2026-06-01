using System.ComponentModel.DataAnnotations;

namespace DBGuard.DataAccess.Data.Entities;

public class Rule
{
    [Key]
    public int Key { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; }

    public string Data { get; set; } = string.Empty;
}