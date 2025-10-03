using System.ComponentModel.DataAnnotations;

namespace CarPark.Api.Models;

public class VehicleType
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
