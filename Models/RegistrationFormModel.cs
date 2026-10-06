using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public sealed class RegistrationFormModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string AttendeeName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public EventRegistration ToRegistration(Guid eventId) => new()
    {
        EventId = eventId,
        AttendeeName = AttendeeName.Trim(),
        Email = Email.Trim(),
        RegisteredAt = DateTime.Now
    };
}
