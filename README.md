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

## Running in Production

```bash
docker compose -f docker-compose.prod.yml up -d
```

This starts the app on port `8080` alongside a MongoDB instance, with named volumes for database data and uploaded images. The app image is pulled from `ghcr.io/paulmfischer/repair-tracker:latest`.
