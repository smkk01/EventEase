using EventEase.Models;

namespace EventEase.Services;

public interface IEventService
{
    Task<IReadOnlyList<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(Guid id);
    Task CreateAsync(Event item);
    Task UpdateAsync(Event item);
    Task DeleteAsync(Guid id);
}
