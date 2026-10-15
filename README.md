# EventEase

EventEase is a browser-based event management app built with Blazor WebAssembly and .NET 8. Browse events, create and edit event details, register attendees, and record attendance with check-in.

## Features

- Browse sample and user-created events.
- Create, edit, and delete events.
- View event details, capacity, registrations, and check-in status.
- Register attendees with name and email validation.
- Enforce event capacity and prevent the same email from registering twice for an event.
- Check in a registered attendee from the event details page.
- Keep event and registration data in the browser using `localStorage`.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run locally

From the repository directory:

```bash
dotnet restore
dotnet run
```

Open the URL printed by `dotnet run`. The configured HTTP launch profile is available at `http://localhost:5129`; the HTTPS profile is available at `https://localhost:7283`.

To build without starting the app:

```bash
dotnet build
```

## Pages

| Route | Description |
| --- | --- |
| `/` or `/events` | Browse, create, edit, and delete events |
| `/events/new` | Create an event |
| `/events/edit/{id}` | Edit an event |
| `/events/{id}` | View event details and attendee attendance; check in your registration |
| `/events/{id}/register` | Register for an event |

## GitHub Pages deployment

The GitHub Actions workflow publishes the app at `https://smkk01.github.io/EventEase/`. Internal links are relative to the app's base path so navigation works under the repository subpath. The workflow also publishes `404.html` as a fallback so directly opening or refreshing an event route loads the Blazor app.

## Data and storage

The app is a client-side Blazor WebAssembly application and does not currently connect to a server-side database or API. On first use in a browser, it loads the initial data from [`wwwroot/sample-data/events.json`](wwwroot/sample-data/events.json) and [`wwwroot/sample-data/registrations.json`](wwwroot/sample-data/registrations.json). Changes are saved in that browser's `localStorage`, and saved data takes precedence over the sample files on later visits.

Data is local to the browser and origin where the app is running; it is not shared between browsers or devices. Clearing the site's browser storage removes the saved events and registrations and allows the sample data to load again.

## Project structure

```text
EventEase.csproj             Blazor WebAssembly project configuration
Pages/                       Event list, details, editor, and registration pages
Models/                      Event, registration, and form models
Services/                    Event/registration storage and browser-session services
wwwroot/sample-data/         Initial sample events and registrations
wwwroot/css/                 App and Bootstrap styles
```
