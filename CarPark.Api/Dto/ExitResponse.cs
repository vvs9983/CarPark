namespace CarPark.Api.Dto;

public class ExitResponse
{
    public string VehicleReg { get; set; } = string.Empty;
    public DateTime TimeIn { get; set; }
    public DateTime TimeOut { get; set; }
    public decimal VehicleCharge { get; set; }
}