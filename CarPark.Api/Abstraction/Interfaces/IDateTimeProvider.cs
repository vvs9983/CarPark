namespace CarPark.Api.Abstraction.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
