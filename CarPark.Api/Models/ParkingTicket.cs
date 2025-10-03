using System.ComponentModel.DataAnnotations;

namespace CarPark.Api.Models;

public class ParkingTicket
{
    [Key]
    public int Id { get; set; }
    public string VehicleReg { get; set; } = string.Empty;
    public int VehicleTypeId { get; set; }
    public VehicleType VehicleType { get; set; } = null!;
    public int SpotNumber { get; set; }
    public DateTime TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public bool IsActive { get; set; }
}
