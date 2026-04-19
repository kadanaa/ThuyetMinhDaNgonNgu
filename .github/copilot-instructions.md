# Copilot Instructions

## Project Guidelines
- User prefers using Azure SQL Database connection string Data Source=jupiserver.database.windows.net; Initial Catalog=ThuyetMinhDaNgonNgu for all apps (Admin, POIOwner, Tourist).

### Tourist App — Map Behavior
- Recenter map to the new standing location when it changes.
- Preserve the current zoom level exactly when recentering for standing-location changes; do not perform any automatic zoom adjustments (no zoom-out or zoom-in).
- Do not auto-expand the bottom sheet when the standing location changes.