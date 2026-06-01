using System.ComponentModel.DataAnnotations;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.DataAccess.Data.Entities;

public class Alert
{
    [Key]
    public int Id { get; set; }
    
    public AlertType Type { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    
    public string IpAddress { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; }
}