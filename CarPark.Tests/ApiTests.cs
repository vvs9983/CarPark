using System.Net;
using System.Net.Http.Json;

using CarPark.Api.Abstraction.Interfaces;
using CarPark.Api.Dto;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CarPark.Tests;

[TestClass]
public sealed class ApiTests
{
    private static WebApplicationFactory<Program> factory = null!;
    private static HttpClient client = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dateTimeProvider = services.SingleOrDefault(d => d.ServiceType == typeof(IDateTimeProvider));

                if (dateTimeProvider != null)
                {
                    services.Remove(dateTimeProvider);
                }

                services.AddSingleton<IDateTimeProvider>(new FakeDateTimeProvider { UtcNow = DateTime.UtcNow });
            });
        });
        client = factory.CreateClient();
    }

    [TestMethod]
    public async Task GetOccupancy_ShouldReturnOccupancyAsync()
    {
        var occupancyResponse = await client.GetFromJsonAsync<OccupancyResponse>("/parking");

        occupancyResponse.Should().NotBeNull();
        occupancyResponse!.AvailableSpaces.Should().BeGreaterThanOrEqualTo(0);
        occupancyResponse.OccupiedSpaces.Should().BeGreaterThanOrEqualTo(0);
    }

    [TestMethod]
    public async Task Park_ShouldReturnBadRequest_ForEmptyVehicleRegAsync()
    {
        var parkRequest = new { VehicleReg = "", VehicleType = "car" };
        var response = await client.PostAsJsonAsync("/parking", parkRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task Park_ShouldReturnBadRequest_ForEmptyVehicleTypeAsync()
    {
        var parkRequest = new { VehicleReg = "ABC123", VehicleType = "" };
        var response = await client.PostAsJsonAsync("/parking", parkRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task Park_ShouldReturnBadRequest_ForInvalidVehicleTypeAsync()
    {
        var parkRequest = new { VehicleReg = "ABC123", VehicleType = "invalid_type" };
        var response = await client.PostAsJsonAsync("/parking", parkRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task Park_ShouldReturnConflict_ForAlreadyParkedVehicleAsync()
    {
        var parkRequest = new { VehicleReg = "XYZ789", VehicleType = "Medium" };
        var firstResponse = await client.PostAsJsonAsync("/parking", parkRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondResponse = await client.PostAsJsonAsync("/parking", parkRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [TestMethod]
    public async Task Park_ShouldReturnBadRequest_WhenNoSpotsAvailableAsync()
    {
        var tasks = new List<Task<HttpResponseMessage>>();
        Random rnd = new();
        var vehicleTypes = new[] { "Small", "Medium", "Large" };
        var responses = new List<HttpResponseMessage>();

        for (int i = 0; i < 105; i++)
        {
            var parkRequest = new { VehicleReg = $"CAR{i}", VehicleType = vehicleTypes[rnd.Next(2)] };
            responses.Add(await client.PostAsJsonAsync("/parking", parkRequest));
        }

        var badRequestCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
        badRequestCount.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public async Task Park_ShouldSucceed_ForValidRequestAsync()
    {
        var parkRequest = new { VehicleReg = "VALID123", VehicleType = "Small" };
        var response = await client.PostAsJsonAsync("/parking", parkRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ticket = await response.Content.ReadFromJsonAsync<ParkResponse>();
        ticket.Should().NotBeNull();
        ticket!.VehicleReg.Should().Be("VALID123");
        ticket.SpotNumber.Should().BeGreaterThan(0);
        ticket.TimeIn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [TestMethod]
    public async Task Exit_ShouldReturnChargeAsync()
    {
        var parkRequest = new ParkRequest { VehicleReg = "EXIT123", VehicleType = "Medium" };
        var parkResponse = await client.PostAsJsonAsync("/parking", parkRequest);

        parkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clock = factory.Services.GetRequiredService<IDateTimeProvider>() as FakeDateTimeProvider;
        clock!.UtcNow = clock.UtcNow.AddMinutes(19); // Simulate 19 minutes of parking

        var exitRequest = new ExitRequest { VehicleReg = "EXIT123" };
        var response = await client.PostAsJsonAsync($"/parking/exit", exitRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exitResponse = await response.Content.ReadFromJsonAsync<ExitResponse>();

        exitResponse.Should().NotBeNull();
        exitResponse!.VehicleReg.Should().Be("EXIT123");
        exitResponse.TimeIn.Should().BeBefore(exitResponse.TimeOut);
        
        Assert.AreEqual(19 * 0.2m + 19 / 5 * 1, exitResponse.VehicleCharge); // 19 minutes for Medium vehicle
    }
}
