# RentKart
## Rent Anything You Need.

RentKart is a local multi-vendor peer-to-peer rental marketplace built with ASP.NET Core MVC. 

## Overview
RentKart allows multiple rental businesses to register, list equipment, and manage their rental operations. Customers can browse local vendors, search for equipment, create bookings, and securely pay online. The MVP focuses on local pickup and return.

## Features
- Multi-Vendor Support
- Role-Based Access (Admin, Vendor, Staff, Customer)
- Real-time Equipment Availability
- Booking & Security Deposit Management
- Rental Lifecycle Tracking (Issue, Return, Damage)
- Real-time Notifications (SignalR)
- Analytics & Reports

## Technology Stack
- **Backend**: ASP.NET Core 10.0 MVC, C#
- **Database**: Entity Framework Core, SQL Server
- **Frontend**: Bootstrap 5, Javascript, Chart.js
- **Real-time**: SignalR

## Architecture
See [ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Database
See [DATABASE.md](docs/DATABASE.md)

## Project Structure
- `RentKart.Core`: Domain entities and interfaces.
- `RentKart.Infrastructure`: EF Core DbContext and Services.
- `RentKart.Web`: Controllers, Views, and Web Host.
- `RentKart.Tests`: Unit tests.

## How to Run
1. Configure `ConnectionStrings:DefaultConnection` in `appsettings.json` or user secrets.
2. Run database migrations: `dotnet ef database update`
3. Run the application: `dotnet run`

## Demo Data
DEMO ACCOUNTS (Development/Demo Only):
- SuperAdmin: admin@rentkart.com
- VendorAdmin: vendor@rentkart.com
- Customer: customer@rentkart.com

## Future Scope (RentKart V2)
- AI Equipment Recommendations
- Mobile Application
- GPS Equipment Tracking
- Vendor Commission System
- Home Delivery

## Authors
Smit Bhalani & Team
