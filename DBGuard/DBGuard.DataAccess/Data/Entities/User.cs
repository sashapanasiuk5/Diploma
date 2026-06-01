using System.ComponentModel.DataAnnotations;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.DataAccess.Data.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = false;
}