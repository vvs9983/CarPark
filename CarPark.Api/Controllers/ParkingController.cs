using CarPark.Api.Abstraction.Interfaces;
using CarPark.Api.Dto;
using CarPark.Api.Models;
using CarPark.Api.Repo;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ParkingController(CarParkDbContext dbContext, IBillingService billingService, IDateTimeProvider dateTimeProvider) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOccupancy()
    {
        var totalSpots = await dbContext.ParkingSpots.CountAsync();
        var occupiedSpots = await dbContext.ParkingSpots.CountAsync(s => s.IsOccupied);
        var availableSpots = totalSpots - occupiedSpots;

        var parkingStatus = new OccupancyResponse
        {
            AvailableSpaces = availableSpots,
            OccupiedSpaces = occupiedSpots
        };

        return Ok(parkingStatus);
    }

    [HttpPost]
    public async Task<IActionResult> Park([FromBody] ParkRequest parkRequest)
    {
        if (string.IsNullOrWhiteSpace(parkRequest.VehicleReg))
        {
            return BadRequest(new { Error = "Vehicle registration number is required." });
        }

        if (string.IsNullOrWhiteSpace(parkRequest.VehicleType))
        {
            return BadRequest(new { Error = "Vehicle type is required." });
        }

        var vehicleType = await Heplers.FindVehicleTypeAsync(dbContext, parkRequest.VehicleType);
        
        if (vehicleType == null)
        {
            return BadRequest(new { Error = "Invalid vehicle type." });
        }

        var existingTicket = await dbContext.ParkingTickets.Where(t => t.VehicleReg == parkRequest.VehicleReg && t.IsActive).FirstOrDefaultAsync();

        if (existingTicket != null)
        {
            return Conflict(new { Error = "Vehicle is already parked." });
        }

        var spot = await dbContext.ParkingSpots.Where(s => !s.IsOccupied).OrderBy(s => s.SpotNumber).FirstOrDefaultAsync();

        if (spot == null)
        {
            return BadRequest(new { Error = "No available parking spots." });
        }

        var now = dateTimeProvider.UtcNow;
        var parkingTicket = new ParkingTicket
        {
            VehicleReg = parkRequest.VehicleReg,
            VehicleTypeId = vehicleType.Id,
            SpotNumber = spot.SpotNumber,
            TimeIn = now,
            IsActive = true
        };

        spot.IsOccupied = true;

        await dbContext.ParkingTickets.AddAsync(parkingTicket);
        await dbContext.SaveChangesAsync();

        var parkResponse = new ParkResponse
        {
            VehicleReg = parkRequest.VehicleReg,
            SpotNumber = spot.SpotNumber,
            TimeIn = now
        };

        return Ok(parkResponse);
    }

    [HttpPost("exit")]
    public async Task<IActionResult> Exit([FromBody] ExitRequest exitRequest)
    {
        if (string.IsNullOrEmpty(exitRequest.VehicleReg))
        {
            return BadRequest(new { Error = "Vehicle registration number is required." });
        }

        var ticket = await dbContext.ParkingTickets
            .Include(t => t.VehicleType)
            .FirstOrDefaultAsync(t => t.VehicleReg.Equals(exitRequest.VehicleReg));

        if (ticket is null)
        {
            return Conflict(new { Error = "Vehicle is not found in the car park" });
        }

        if (!ticket.IsActive)
        {
            return Conflict(new { Error = "Vehicle has already exited the car park." });
        }

        var spot = await dbContext.ParkingSpots.FirstOrDefaultAsync(s => s.SpotNumber == ticket.SpotNumber);

        if (spot is null)
        {
            return Conflict(new { Error = "Parking spot associated with the ticket is not found." });
        }

        if (!spot.IsOccupied)
        {
            return Conflict(new { Error = "Parking spot is already marked as unoccupied." });
        }

        var vehicleType = await dbContext.VehicleTypes.FirstOrDefaultAsync(v => v.Id == ticket.VehicleTypeId);

        var now = dateTimeProvider.UtcNow;
        var totalCharge = await billingService.CalculateChargeAsync(ticket.VehicleType, ticket.TimeIn, now);

        ticket.TimeOut = now;
        ticket.IsActive = false;
        spot.IsOccupied = false;

        await dbContext.SaveChangesAsync();

        var exitResponse = new ExitResponse
        {
            VehicleReg = ticket.VehicleReg,
            TimeIn = ticket.TimeIn,
            TimeOut = now,
            VehicleCharge = totalCharge
        };

        return Ok(exitResponse);
    }
}
