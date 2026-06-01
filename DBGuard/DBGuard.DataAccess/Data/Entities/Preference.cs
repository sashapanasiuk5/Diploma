using System.ComponentModel.DataAnnotations;

namespace DBGuard.DataAccess.Data.Entities;

public class Preference
{
    [Key]
    public int Id { get; set; }
    
    public string Data { get; set; } = string.Empty;
}