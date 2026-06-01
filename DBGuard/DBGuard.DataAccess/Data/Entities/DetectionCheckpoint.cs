using System.ComponentModel.DataAnnotations;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.DataAccess.Data.Entities;

public class DetectionCheckpoint
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DetectionType Type { get; set; }

    [Required]
    [MaxLength(256)]
    public string EntityValue { get; set; }

    [Required]
    public DateTime LastAlertTimestamp { get; set; }
}