# Repair Tracker

A personal web app for tracking broken electronics bought for repair and resale. Replaces a spreadsheet with a proper UI, repair status workflow, and persistent storage.

## Features

- **Item tracking** — log purchases with cost, source, fault description, and repair notes with photos
- **Status workflow** — progress items through Intake → Diagnosis → Parts Ordered → Repaired → Listed → Sold
- **Financial tracking** — track cost, parts, sell price, postage, and hours worked; auto-compute net profit and hourly rate
- **Dashboard** — aggregated profit totals and item counts by status
- **Calculator** — pre-purchase profit estimator
- **Settings** — configurable reseller/platform fee percentage

## Tech Stack

- .NET 10 Blazor Web App (Interactive Server)
- MudBlazor UI components
- MongoDB

## Running Locally

```bash
# Start MongoDB
podman compose up -d

# Run the app
dotnet run --project RepairTracker/RepairTracker.csproj
```

Configure the MongoDB connection in `appsettings.json` (`ConnectionStrings:MongoDB` and `MongoDB:Database`).

MongoDB runs with authentication enabled. On first start with an empty volume, `mongo-init/01-create-app-user.js` creates a
`repairtracker` user with `readWrite` on the `RepairTracker` database only. The dev compose uses dev-only default passwords
(override with `MONGO_ROOT_PASSWORD` / `MONGO_APP_PASSWORD`) and `appsettings.Development.json` matches them.

The init script only runs against an empty data volume. If you already have a dev volume, either recreate it
(`podman compose down -v`, which deletes the data) or create the user by hand with `mongosh` (needs a user that can create users,
so on an existing no-auth volume this works only before auth is enabled on that instance).

## Running in Production

```bash
docker compose -f docker-compose.prod.yml up -d
```

`MONGO_ROOT_PASSWORD` and `MONGO_APP_PASSWORD` must be set (for example in a `.env` file next to the compose file); they are
applied when the MongoDB volume is first created, and the app connects as the `repairtracker` user.

This starts the app on port `8080` alongside a MongoDB instance, with named volumes for database data and uploaded images. The app image is pulled from `ghcr.io/paulmfischer/repair-tracker:latest`.
