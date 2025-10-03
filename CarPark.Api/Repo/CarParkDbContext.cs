using CarPark.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace CarPark.Api.Repo;

public class CarParkDbContext(DbContextOptions<CarParkDbContext> options) : DbContext(options)
{
    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
    public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
}
