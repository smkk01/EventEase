using Microsoft.JSInterop;

namespace EventEase.Services;

public sealed class BrowserSessionService(IJSRuntime jsRuntime)
{
    private const string SessionIdKey = "eventEase.session.id.v1";
    private const string RegistrationKeyPrefix = "eventEase.session.registration.v1.";
    private string? _sessionId;

    public async Task<string> GetSessionIdAsync()
    {
        if (_sessionId is not null)
        {
            return _sessionId;
        }

        var storedSessionId = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", SessionIdKey);
        if (storedSessionId is null)
        {
            _sessionId = Guid.NewGuid().ToString("N");
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", SessionIdKey, _sessionId);
        }
        else if (Guid.TryParseExact(storedSessionId, "N", out _))
        {
            _sessionId = storedSessionId;
        }
        else
        {
            throw new InvalidDataException("The saved browser session ID is invalid.");
        }

        return _sessionId;
    }

    public async Task<Guid?> GetRegistrationIdAsync(Guid eventId)
    {
        await GetSessionIdAsync();
        var storedId = await jsRuntime.InvokeAsync<string?>(
            "sessionStorage.getItem",
            $"{RegistrationKeyPrefix}{eventId:N}");

        if (storedId is null)
        {
            return null;
        }

        return Guid.TryParse(storedId, out var registrationId)
            ? registrationId
            : throw new InvalidDataException($"The saved session registration for event '{eventId}' is invalid.");
    }

    public async Task AssociateRegistrationAsync(Guid eventId, Guid registrationId)
    {
        await GetSessionIdAsync();
        await jsRuntime.InvokeVoidAsync(
            "sessionStorage.setItem",
            $"{RegistrationKeyPrefix}{eventId:N}",
            registrationId.ToString("D"));
    }
}
