# Repair Tracker — Blazor + MongoDB Application Plan

## Context

The user has a spreadsheet ("Sally's Spectacular Spreadsheet") that tracks broken electronics bought for repair and resale. The app replaces the spreadsheet with a proper web UI, richer repair status workflow, and persistent MongoDB storage. The app is platform-agnostic — a configurable reseller fee replaces the hardcoded eBay 9% fee, and listing references are generic (not eBay-specific). No authentication is needed (single-user personal tool).

---

## Spreadsheet → App Mapping

| Spreadsheet Tab | App Equivalent |
|---|---|
| Introduction | — (replaced by the app itself) |
| Calculator | Calculator page — live profit estimator |
| Faults | Merged into Item Detail — repair notes + status workflow |
| Items | Items List + Item Detail pages |
| Total Profit | Dashboard — aggregated stats |
| Final | Dashboard — summary totals vs. target |

---

## Data Models

### `ItemSource` enum
```
eBay, GameStore, FacebookMarketplace, Craigslist, Local, Other
```

### `Item` (MongoDB collection: `items`)
| Field | Type | Notes |
|---|---|---|
| Id | string | MongoDB ObjectId |
| Name | string | Item description |
| SerialNumber | string? | Optional serial number |
| Source | `ItemSource` enum | Where the item was acquired |
| PurchaseListingId | string | Listing reference where bought (any platform) |
| SaleListingId | string | Listing reference where sold (any platform) |
| Status | `RepairStatus` enum | See workflow below |
| OriginalFault | string | Issue found on arrival |
| RepairNotes | string | How it was fixed |
| Cost | decimal | Purchase price paid |
| Parts | decimal | Cost of parts used |
| EstimatedSellPrice | decimal | Pre-repair sell price estimate |
| ActualSellPrice | decimal | Final sell price received (after platform fees) |
| Postage | decimal | Postage/shipping cost |
| HoursWorked | decimal | Optional time tracking |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

**Computed (not stored):**
- `ResellerFee` = `EstimatedSellPrice × (globalFee% / 100)` (used in estimated profit)
- `EstimatedProfit` = `EstimatedSellPrice - Cost - Parts - ResellerFee`
- `NetProfit` = `ActualSellPrice - Cost - Parts - Postage` (ActualSellPrice is post-fee)
- `HourlyProfit` = `NetProfit / HoursWorked` (if HoursWorked > 0)

### `RepairStatus` enum
```
Intake → Diagnosis → PartsOrdered → Repaired → Listed → Sold
```

### `AppSettings` (MongoDB collection: `settings`, single document)
| Field | Type | Default | Notes |
|---|---|---|---|
| ResellerFeePercentage | decimal | 9.0 | Applies to any resale platform |

---

## Application Structure

```
RepairTracker/
├── RepairTracker.sln
└── RepairTracker/
    ├── Components/
    │   ├── App.razor
    │   ├── Routes.razor
    │   ├── Layout/
    │   │   ├── MainLayout.razor      ← MudBlazor shell: nav drawer + app bar
    │   │   └── NavMenu.razor
    │   └── Pages/
    │       ├── Dashboard.razor       ← summary stats, profit vs target
    │       ├── Calculator.razor      ← pre-purchase what-if tool
    │       ├── Items.razor           ← data table with status filter
    │       ├── ItemDetail.razor      ← create/edit item + status stepper
    │       └── Settings.razor        ← reseller fee %, profit target
    ├── Models/
    │   ├── Item.cs
    │   ├── AppSettings.cs
    │   ├── RepairStatus.cs
    │   └── ItemSource.cs
    ├── Services/
    │   ├── Interfaces/
    │   │   ├── IItemService.cs
    │   │   └── ISettingsService.cs
    │   ├── ItemService.cs
    │   └── SettingsService.cs
    ├── Data/
    │   └── MongoDbContext.cs         ← wraps IMongoDatabase, exposes typed collections
    ├── appsettings.json              ← MongoDB connection string + DB name
    └── Program.cs                    ← DI registration: MongoDB, services, MudBlazor
```

---

## Key Pages

### Dashboard
- Cards: Estimated Total Profit, Actual Total Profit, Total Postage, Total Hours Worked, Hourly Rate
- Status count badges (items by RepairStatus)

### Calculator
- Stateless form: Cost, Parts, Estimated Sell Price inputs
- Auto-computed: Reseller Fee (reads global setting), Gross Profit
- "Save as new item" button optionally creates an Item record at Intake status

### Items List
- MudDataGrid with columns: Name, Status chip, Cost, Est. Profit, Net Profit, Hours
- Filter bar: RepairStatus multi-select, search by name
- Row click → ItemDetail

### Item Detail
- MudStepper for status progression (Intake → Sold)
- Two-section form: Repair Info (fault, notes, hours) + Financial Info (cost, parts, prices, postage)
- All computed fields shown read-only inline

### Settings
- ResellerFeePercentage with label "Reseller/Platform Fee %" (MudNumericField)
- Save persists to MongoDB `settings` collection

---

## Tech Stack

- **.NET 10 Blazor Web App** — Interactive Server rendering mode
- **MongoDB.Driver** (official C# driver) — `IMongoCollection<T>` directly, no ORM
- **MudBlazor** — UI component library
- **MongoDB** — local or Atlas connection via `appsettings.json`

---

## Implementation Steps

- [ ] 1. `dotnet new blazorweb -n RepairTracker --interactivity Server` — scaffold project
- [ ] 2. Add NuGet packages: `MongoDB.Driver`, `MudBlazor`
- [ ] 3. Configure MudBlazor in `Program.cs` and `App.razor`
- [ ] 4. Create `MongoDbContext`, register in DI with connection string from `appsettings.json`
- [ ] 5. Implement `Models/` (Item, AppSettings, RepairStatus enum, ItemSource enum)
- [ ] 6. Implement `IItemService` / `ItemService` with CRUD + aggregation queries
- [ ] 7. Implement `ISettingsService` / `SettingsService` (upsert single settings doc)
- [ ] 8. Build `MainLayout` with MudBlazor nav drawer
- [ ] 9. Build pages in order: Settings → Calculator → Items → ItemDetail → Dashboard
- [ ] 10. Wire up computed profit fields as C# properties on the model or in service layer

---

## Verification

- Run `dotnet run` and navigate to `https://localhost:5001`
- Create a new item via ItemDetail, advance through all repair statuses
- Use Calculator, verify gross profit formula matches expected values
- Change reseller fee % in Settings, confirm Calculator reflects new value
- Verify Dashboard totals match hand-calculated sums from the Items list
- Check MongoDB directly with `mongosh` to confirm documents are persisted correctly
