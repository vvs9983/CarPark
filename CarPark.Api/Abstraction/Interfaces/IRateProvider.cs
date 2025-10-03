namespace CarPark.Api.Abstraction.Interfaces;

public interface IRateProvider
{
    public record PricingRate(decimal RatePerUnit,  int ChargeUnitSeconds, string Rounding, int SurchargeEveryNSeconds, decimal SurchargeAmount);

    Task<PricingRate?> GetPricingRateAsync(int vehicleTypeId);
}
