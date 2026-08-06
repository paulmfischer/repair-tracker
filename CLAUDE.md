# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Repair Tracker is a personal single-user web app for tracking broken electronics bought for repair and resale. It replaces a spreadsheet with a Blazor UI backed by MongoDB. No authentication is required.

## Tech Stack

- **.NET 10 Blazor Web App** — hybrid render model: `RepairTracker.Server` server-renders the host page and exposes HTTP endpoints; `RepairTracker.Client` is a `Microsoft.NET.Sdk.BlazorWebAssembly` project that boots in the browser with `AddInteractiveWebAssemblyRenderMode()` (`prerender: false`) and owns all client-side routing/navigation after boot
- **MudBlazor 8.8.0** — all UI components come from MudBlazor; do not mix in raw HTML or other component libraries
- **MongoDB.Driver 3.10.0** — `IMongoCollection<T>` directly; no ORM or repository abstraction beyond `MongoDbContext`. Only `RepairTracker.Server` touches MongoDB — the WASM client never does

## Commands

```bash
# Start MongoDB (required before running the app)
podman compose up -d

# Run the app
dotnet run --project RepairTracker.Server/RepairTracker.Server.csproj

# Build without running
dotnet build RepairTracker.Server/RepairTracker.Server.csproj

# Restore packages
dotnet restore RepairTracker.Server/RepairTracker.Server.csproj
```

There are no tests in this project.

## Architecture

Three projects, all targeting `net10.0`:

```
RepairTracker.Shared/                 # Models + service interfaces, referenced by both Client and Server
├── Models/                           # Plain C# models; computed props marked [BsonIgnore]
└── Services/Interfaces/              # IItemService, ISettingsService, IReportService

RepairTracker.Client/                 # Microsoft.NET.Sdk.BlazorWebAssembly — runs in the browser
├── Program.cs                        # DI wiring: MudBlazor, HttpClient, Api*Service implementations
├── Pages/                            # One .razor per page; @code block inline
├── Layout/                           # MainLayout (nav drawer + app bar), NavMenu
├── Shared/                           # Reusable components (RepairLog, RepairStepper, ImagePreviewDialog)
└── Services/
    ├── ApiItemService.cs             # IItemService over HTTP — calls RepairTracker.Server's api/items endpoints
    └── ApiSettingsService.cs         # ISettingsService over HTTP

RepairTracker.Server/                 # Microsoft.NET.Sdk.Web — hosts the app, owns all MongoDB access
├── Program.cs                        # DI wiring: MongoDB, Razor Components + WASM render mode, endpoints
├── Data/
│   └── MongoDbContext.cs             # Wraps IMongoDatabase; exposes Items and Settings collections
├── Services/
│   ├── ItemService.cs                # CRUD + dashboard aggregation (loads all items into memory)
│   └── SettingsService.cs            # Upserts single AppSettings document
├── Endpoints/                        # Minimal API endpoints the Client's Api*Service classes call
│   ├── ItemsEndpoints.cs, SettingsEndpoints.cs, ImagesEndpoints.cs, ReportEndpoints.cs
├── Components/
│   └── App.razor                     # The one server-rendered host page; boots the WASM runtime
└── wwwroot/                          # Static assets: app.css, favicon.png, manifest.json, service-worker.js, js/
```

Since the WASM client can't touch the server's filesystem or MongoDB directly, anything requiring either (image uploads, note images, CRUD against Mongo) goes through an HTTP endpoint in `RepairTracker.Server/Endpoints/`, called from a matching `Api*Service` class in `RepairTracker.Client/Services/`.

### Offline support

The app has a service worker (`RepairTracker.Server/wwwroot/service-worker.js`) and web manifest (`RepairTracker.Server/wwwroot/manifest.json`) so it's installable and can reload/launch with no network — it lazily caches static assets and the most recent successful navigation response as it fetches them (no build-time precache list, since there's no static `index.html` to precache in this hybrid render model). `RepairTracker.Client/Services/ConnectivityService.cs` wraps `navigator.onLine`/`online`/`offline` events (via `wwwroot/js/connectivity.js`) and drives the offline banner in `MainLayout.razor`.

Item and settings data are also cached client-side in IndexedDB (via `wwwroot/js/offlineDb.js` and `RepairTracker.Client/Services/IndexedDbStore.cs`) so they're browsable offline. `CachingItemService`/`CachingSettingsService` implement `IItemService`/`ISettingsService` as decorators around `ApiItemService`/`ApiSettingsService` — try the network, write through to IndexedDB on success, fall back to the cache on failure — and are what `Program.cs` actually registers behind the interfaces; `.razor` pages never talk to `Api*Service` directly. `GetDashboardStatsAsync` falls back to recomputing the aggregation client-side from cached items using the same `Item` extension methods the server uses.

Writes made offline (create/update/delete an item, save settings) are applied optimistically to the IndexedDB cache and queued as an `OutboxOperation` (`RepairTracker.Client/Services/OutboxStore.cs`) instead of failing outright. `OutboxSyncService` replays the queue in order against the raw `Api*Service` classes once `ConnectivityService` reports back online (and once at app startup, in case anything was left over from a previous offline session), showing a snackbar with how many changes synced. `OutboxStore` collapses a delete against a still-queued create for the same item so nothing ever reaches the server for something created and deleted entirely offline. Server-side, `ItemService.CreateAsync` upserts by the client-minted `Id` rather than inserting, so a replayed create is idempotent. Offline image uploads are explicitly out of scope: `RepairLog.razor`'s "Attach Pictures" control is disabled while offline with a message to reconnect.

### Data model key points

- `Item` is the core document stored in the `items` collection. Notes are embedded as `List<RepairNote>` (not a separate collection).
- `RepairNote` stores the status at time of note creation (`StatusAtTime`) plus optional image paths. Images are written to disk under `{itemId}/{noteId}/` inside an uploads root that's deliberately kept outside `wwwroot` (see `RepairTracker.Server/UploadsPath.cs`) so runtime writes don't trigger the dev-time Static Web Assets file watcher.
- `AppSettings` is a singleton document in the `settings` collection; `SettingsService` upserts it on every save and creates a default on first read.
- Computed financials (`NetProfit`, `HourlyProfit`, `EstimatedProfit`, `ResellerFee`) live as methods/properties on `Item` — not persisted. `[BsonIgnore]` is needed only on properties, not methods.
- `ResellerFeePercentage` is read from `AppSettings` at page load and passed into `Item` methods — it is not stored on `Item` itself.

### Configuration

MongoDB connection string and database name come from `appsettings.json` (`ConnectionStrings:MongoDB` and `MongoDB:Database`). Override for local dev in `appsettings.Development.json`.

## Commit Guidelines

- Use [Conventional Commits](https://www.conventionalcommits.org/) syntax: `feat:`, `fix:`, `chore:`, `refactor:`, `docs:`, etc.
- Do not add "Co-authored-by" or "Generated by AI" attributions to commits.
