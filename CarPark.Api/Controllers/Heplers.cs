using CarPark.Api.Models;
using CarPark.Api.Repo;

using Microsoft.EntityFrameworkCore;

namespace CarPark.Api.Controllers;

public class Heplers
{
    public static async Task<VehicleType?> FindVehicleTypeAsync(CarParkDbContext dbContext, string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return null;

        var typeNameTrimmed = typeName.ToLower().Trim();
        var vehicleType = await dbContext.VehicleTypes.FirstOrDefaultAsync(vt => vt.Key == typeName);        

        return vehicleType;
    }
}
