# System Architecture
RentKart is built using a classic N-Tier Layered Architecture with ASP.NET Core MVC.

## Layered Architecture
- **Web Layer (RentKart.Web)**: Contains MVC Controllers, Razor Views, ViewModels, and SignalR Hubs.
- **Service Layer (RentKart.Infrastructure/Services)**: Contains the core business logic (BookingService, PaymentService, etc.).
- **Data Access Layer (RentKart.Infrastructure/Data)**: Contains EF Core ApplicationDbContext and Repository abstractions.
- **Domain Layer (RentKart.Core)**: Contains Entities, Interfaces, and Enums.

## Flows
- **Authentication**: Uses ASP.NET Core Identity with cookie-based sessions.
- **Authorization**: Role-based access control (RBAC). Roles: SuperAdmin, VendorAdmin, VendorStaff, Customer.
- **Booking Flow**: Customer selects equipment -> checks availability -> creates booking -> pays -> Vendor accepts.
- **Rental Flow**: Equipment Issued -> Active -> Returned -> Inspected -> Damage reported (if any) -> Deposit settled -> Completed.
- **Notification Flow**: Action occurs -> Notification created in DB -> SignalR pushes to active browser clients.
