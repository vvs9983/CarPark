
namespace CarPark.Api.Services;

public class DateTimeProvider : Abstraction.Interfaces.IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
