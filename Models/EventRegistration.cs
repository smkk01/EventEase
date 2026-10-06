namespace EventEase.Models;

public sealed record EventRegistration
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid EventId { get; init; }
    public string AttendeeName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; } = DateTime.Now;
    public DateTime? CheckedInAt { get; init; }
}
