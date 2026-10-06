using EventEase.Models;

namespace EventEase.Services;

public interface IRegistrationService
{
    Task<IReadOnlyList<EventRegistration>> GetByEventIdAsync(Guid eventId);
    Task RegisterAsync(EventRegistration registration, int capacity);
    Task<EventRegistration> CheckInAsync(Guid eventId, Guid registrationId);
    Task DeleteForEventAsync(Guid eventId);
}
