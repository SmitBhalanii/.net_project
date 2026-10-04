# Database Documentation
RentKart uses Microsoft SQL Server managed via Entity Framework Core Code-First Migrations.

## Important Tables
- **AspNetUsers / AspNetRoles**: Manages Identity.
- **Vendors**: Represents rental businesses.
- **Equipment / Categories**: The catalog of rentable items.
- **Bookings**: Transactional records bridging Customers and Equipment.
- **Rentals / DamageReports**: Lifecycle records for the physical handover of equipment.
- **Payments / Invoices / Refunds**: Financial ledgers.
- **Notifications**: System and real-time alerts.

## Migrations
To apply migrations:
```bash
dotnet ef database update
```
