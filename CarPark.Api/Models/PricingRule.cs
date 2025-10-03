namespace CarPark.Api.Models;

public class PricingRule
{
    public int Id { get; set; }
    public int VehicleTypeId { get; set; }
    public VehicleType VehicleType { get; set; } = null!;
    public decimal RatePerUnit { get; set; } = 1.0m;
    public int ChargeUnitSeconds { get; set; } = 60;
    public string Rounding { get; set; } = "Ceil"; // Options: "Ceil", "Floor", "Round"
    public int SurchargeEveryNSeconds { get; set; } = 5 * 60;
    public decimal SurchargeAmount { get; set; } = 1.0m;
    public string Currency { get; set; } = string.Empty;
}
