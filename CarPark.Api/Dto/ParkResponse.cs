namespace CarPark.Api.Dto;

public class ParkResponse
{
    public string VehicleReg { get; set; } = string.Empty;
    public int SpotNumber { get; set; }
    public DateTime TimeIn { get; set; }
}
