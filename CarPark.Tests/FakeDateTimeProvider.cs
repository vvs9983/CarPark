using CarPark.Api.Abstraction.Interfaces;

namespace CarPark.Tests;

internal class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
