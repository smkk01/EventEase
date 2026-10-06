using System.Net.Http.Json;
using System.Text.Json;
using EventEase.Models;
using Microsoft.JSInterop;

namespace EventEase.Services;

public sealed class JsonEventService(HttpClient httpClient, IJSRuntime jsRuntime) : IEventService
{
    private const string StorageKey = "eventEase.events.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly SemaphoreSlim _mutationLock = new(1, 1);
    private List<Event> _events = [];
    private bool _initialized;

    public async Task<IReadOnlyList<Event>> GetAllAsync()
    {
        await EnsureInitializedAsync();
        return _events.OrderBy(item => item.StartsAt).ToArray();
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        await EnsureInitializedAsync();
        return _events.FirstOrDefault(item => item.Id == id);
    }

    public async Task CreateAsync(Event item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await EnsureInitializedAsync();
        await _mutationLock.WaitAsync();
        try
        {
            if (_events.Any(existing => existing.Id == item.Id))
            {
                throw new InvalidOperationException($"An event with ID '{item.Id}' already exists.");
            }

            List<Event> updated = [.. _events, item];
            await PersistAsync(updated);
            _events = updated;
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    public async Task UpdateAsync(Event item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await EnsureInitializedAsync();
        await _mutationLock.WaitAsync();
        try
        {
            var index = _events.FindIndex(existing => existing.Id == item.Id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Event '{item.Id}' was not found.");
            }

            List<Event> updated = [.. _events];
            updated[index] = item;
            await PersistAsync(updated);
            _events = updated;
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureInitializedAsync();
        await _mutationLock.WaitAsync();
        try
        {
            var index = _events.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Event '{id}' was not found.");
            }

            List<Event> updated = [.. _events];
            updated.RemoveAt(index);
            await PersistAsync(updated);
            _events = updated;
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
            if (storedJson is not null)
            {
                _events = JsonSerializer.Deserialize<List<Event>>(storedJson, JsonOptions)
                    ?? throw new InvalidDataException("The saved event data is empty or invalid.");
            }
            else
            {
                _events = await httpClient.GetFromJsonAsync<List<Event>>("sample-data/events.json", JsonOptions)
                    ?? throw new InvalidDataException("The sample event data is empty or invalid.");
            }

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task PersistAsync(IReadOnlyList<Event> events)
    {
        var serialized = JsonSerializer.Serialize(events, JsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, serialized);
    }
}
