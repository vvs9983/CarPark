using System.ComponentModel.DataAnnotations;

namespace CarPark.Api.Models;

public class ParkingSpot
{
    [Key]
    public int SpotNumber { get; set; }
    public bool IsOccupied { get; set; }
}
