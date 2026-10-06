using System.Net.Http.Json;
using System.Text.Json;
using EventEase.Models;
using Microsoft.JSInterop;

namespace EventEase.Services;

public sealed class JsonRegistrationService(HttpClient httpClient, IJSRuntime jsRuntime) : IRegistrationService
{
    private const string StorageKey = "eventEase.registrations.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly SemaphoreSlim _mutationLock = new(1, 1);
    private List<EventRegistration> _registrations = [];
    private bool _initialized;

    public async Task<IReadOnlyList<EventRegistration>> GetByEventIdAsync(Guid eventId)
    {
        await EnsureInitializedAsync();
        return _registrations
            .Where(registration => registration.EventId == eventId)
            .OrderByDescending(registration => registration.RegisteredAt)
            .ToArray();
    }

    public async Task RegisterAsync(EventRegistration registration, int capacity)
    {
        ArgumentNullException.ThrowIfNull(registration);
        await EnsureInitializedAsync();
        await _mutationLock.WaitAsync();
        try
        {
            var eventRegistrations = _registrations.Where(item => item.EventId == registration.EventId).ToArray();
            if (eventRegistrations.Length >= capacity)
            {
                throw new InvalidOperationException("This event is already at capacity.");
            }

            if (eventRegistrations.Any(item => string.Equals(item.Email, registration.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("This email address is already registered for the event.");
            }

            List<EventRegistration> updated = [.. _registrations, registration];
            await PersistAsync(updated);
            _registrations = updated;
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    public async Task<EventRegistration> CheckInAsync(Guid eventId, Guid registrationId)
    {
        await EnsureInitializedAsync();
        await _mutationLock.WaitAsync();
        try
        {
            var index = _registrations.FindIndex(item => item.Id == registrationId && item.EventId == eventId);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Registration '{registrationId}' was not found for event '{eventId}'.");
            }

            var registration = _registrations[index];
            if (registration.CheckedInAt is not null)
            {
                return registration;
            }

            var checkedInRegistration = registration with { CheckedInAt = DateTime.Now };
            List<EventRegistration> updated = [.. _registrations];
            updated[index] = checkedInRegistration;
            await PersistAsync(updated);
            _registrations = updated;
            return checkedInRegistration;
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    public async Task DeleteForEventAsync(Guid eventId)
    {
        await EnsureInitializedAsync();
        await _mutationLock.WaitAsync();
        try
        {
            List<EventRegistration> updated = [.. _registrations.Where(item => item.EventId != eventId)];
            await PersistAsync(updated);
            _registrations = updated;
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            var storedJson = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            _registrations = storedJson is not null
                ? JsonSerializer.Deserialize<List<EventRegistration>>(storedJson, JsonOptions)
                    ?? throw new InvalidDataException("The saved registration data is empty or invalid.")
                : await httpClient.GetFromJsonAsync<List<EventRegistration>>("sample-data/registrations.json", JsonOptions)
                    ?? throw new InvalidDataException("The sample registration data is empty or invalid.");

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task PersistAsync(IReadOnlyList<EventRegistration> registrations)
    {
        var serialized = JsonSerializer.Serialize(registrations, JsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, serialized);
    }
}
