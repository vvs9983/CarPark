using CarPark.Api.Models;

namespace CarPark.Api.Abstraction.Interfaces;

public interface IBillingService
{
    Task<decimal> CalculateChargeAsync(VehicleType vehicleType, DateTime timeInUtc, DateTime timeOutUtc);
}
