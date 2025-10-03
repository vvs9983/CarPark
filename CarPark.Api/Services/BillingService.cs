using CarPark.Api.Abstraction.Interfaces;
using CarPark.Api.Models;

namespace CarPark.Api.Services;

public class BillingService(IRateProvider rateProvider) : IBillingService
{
    public async Task<decimal> CalculateChargeAsync(VehicleType vehicleType, DateTime timeInUtc, DateTime timeOutUtc)
    {
        if (timeOutUtc <= timeInUtc)
        {
            return 0m;
        }

        var rate = await rateProvider.GetPricingRateAsync(vehicleType.Id) ?? throw new InvalidOperationException($"No pricing rate found for vehicle type ID {vehicleType.Key}");
        var totalSeconds = Math.Max(0, (timeOutUtc - timeInUtc).TotalSeconds);
        var unitsExact = totalSeconds / rate.ChargeUnitSeconds;
        var units = rate.Rounding.ToLower() switch
        {
            "ceil" => (int)Math.Ceiling(unitsExact),
            "floor" => (int)Math.Floor(unitsExact),
            "round" => (int)Math.Round(unitsExact, MidpointRounding.AwayFromZero),
            _ => throw new InvalidOperationException($"Invalid rounding method: {rate.Rounding}")
        };

        var baseCost = units * rate.RatePerUnit;
        var surcharge = 0m;
        
        if (rate.SurchargeEveryNSeconds > 0 && rate.SurchargeAmount > 0)
        {
            var surchargeCount = (int)(totalSeconds / rate.SurchargeEveryNSeconds);
            surcharge = surchargeCount * rate.SurchargeAmount;
        }

        var totalCost = baseCost + surcharge;

        return Math.Round(totalCost, 2, MidpointRounding.AwayFromZero);
    }
}
