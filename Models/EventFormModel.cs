using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public sealed class EventFormModel : IValidatableObject
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public DateTime? StartsAt { get; set; } = DateTime.Now.AddDays(7);

    [Required]
    public DateTime? EndsAt { get; set; } = DateTime.Now.AddDays(7).AddHours(1);

    [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100,000.")]
    public int Capacity { get; set; } = 50;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartsAt is not null && EndsAt is not null && EndsAt <= StartsAt)
        {
            yield return new ValidationResult(
                "End time must be later than the start time.",
                [nameof(EndsAt)]);
        }
    }

    public static EventFormModel FromEvent(Event item) => new()
    {
        Title = item.Title,
        Description = item.Description,
        Location = item.Location,
        Category = item.Category,
        StartsAt = item.StartsAt,
        EndsAt = item.EndsAt,
        Capacity = item.Capacity
    };

    public Event ToEvent(Guid id) => new()
    {
        Id = id,
        Title = Title.Trim(),
        Description = Description.Trim(),
        Location = Location.Trim(),
        Category = Category.Trim(),
        StartsAt = StartsAt!.Value,
        EndsAt = EndsAt!.Value,
        Capacity = Capacity
    };
}
