using CarPark.Api.Abstraction.Interfaces;
using CarPark.Api.Repo;

using Microsoft.EntityFrameworkCore;

using static CarPark.Api.Abstraction.Interfaces.IRateProvider;

namespace CarPark.Api.Services;

public class RateProvider(CarParkDbContext dbContext) : IRateProvider
{
    public async Task<PricingRate?> GetPricingRateAsync(int vehicleTypeId)
    {
        var pricingRule = await dbContext.PricingRules.AsNoTracking().FirstOrDefaultAsync(pr => pr.VehicleTypeId == vehicleTypeId);

        return pricingRule is null ? null :
            new PricingRate(
                pricingRule.RatePerUnit,
                pricingRule.ChargeUnitSeconds, 
                pricingRule.Rounding, 
                pricingRule.SurchargeEveryNSeconds,
                pricingRule.SurchargeAmount);
    }
}
