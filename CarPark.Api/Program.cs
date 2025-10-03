using CarPark.Api.Abstraction.Interfaces;
using CarPark.Api.Models;
using CarPark.Api.Repo;
using CarPark.Api.Services;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddDbContext<CarParkDbContext>(c => c.UseInMemoryDatabase("CarParkDb"));
builder.Services.AddScoped<IRateProvider, RateProvider>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(e =>
    {
        e.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { Error = "An unexpected error occurred." });
        });
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CarParkDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!await dbContext.ParkingSpots.AnyAsync())
    {
        var capacity = 100;
        var envCapacityString = Environment.GetEnvironmentVariable("CAR_PARK_SPOTS_COUNT");

        if (int.TryParse(envCapacityString, out int envCapacity))
        {
            capacity = envCapacity;
        }

        for (int i = 1; i <= capacity; i++)
        {
            dbContext.ParkingSpots.Add(new ParkingSpot
            {
                SpotNumber = i,
                IsOccupied = false
            });
        }

        await dbContext.SaveChangesAsync();
    }

    if (!await dbContext.VehicleTypes.AnyAsync())
    {
        var vehicleTypes = new List<VehicleType>
        {
            new() { Key = "Small", DisplayName = "Small Car" },
            new() { Key = "Medium", DisplayName = "Medium Car" },
            new() { Key = "Large", DisplayName = "Large Car" }
        };

        dbContext.VehicleTypes.AddRange(vehicleTypes);

        await dbContext.SaveChangesAsync();
    }

    if (!await dbContext.PricingRules.AnyAsync())
    {
        var small = await dbContext.VehicleTypes.FirstAsync(v => v.Key == "Small");
        var medium = await dbContext.VehicleTypes.FirstAsync(v => v.Key == "Medium");
        var large = await dbContext.VehicleTypes.FirstAsync(v => v.Key == "Large");

        var pricingRules = new List<PricingRule>
        {
            new() { VehicleTypeId = small.Id, RatePerUnit = 0.10m, Currency = "\u00A3" },
            new() { VehicleTypeId = medium.Id, RatePerUnit = 0.20m, Currency = "\u00A3"},
            new() { VehicleTypeId = large.Id, RatePerUnit = 0.40m, Currency = "\u00A3" }
        };

        await dbContext.PricingRules.AddRangeAsync(pricingRules);
        await dbContext.SaveChangesAsync();
    }
}

app.Run();

public partial class Program { }