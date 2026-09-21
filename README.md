# RentKart

**Rent Anything You Need.**

RentKart is a local rental marketplace connecting customers with nearby rental businesses. 

## Current Development Status
- **Phase 0:** Project Constitution & Architecture established.
- **Phase 1:** Solution foundation and ASP.NET Core MVC project structure scaffolding complete. (Current)

## Technology Stack
- C# 13, .NET 9.0 (SDK 10.0.401 installed)
- ASP.NET Core MVC
- Entity Framework Core with SQL Server
- Bootstrap 5

## Project Structure
- `RentKart.Web`: Presentation layer
- `RentKart.Infrastructure`: Data access and configurations
- `RentKart.Core`: Domain models
- `RentKart.Tests`: xUnit tests

## How to Run
1. Install .NET SDK
2. Open terminal in `RentKart` directory
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet run --project RentKart.Web`

## Database Prerequisites
- SQL Server (LocalDB configured by default in `appsettings.json`)
- No schema has been created yet.

## Documentation
- [Architecture](ARCHITECTURE.md)
- [Project Constitution](PROJECT_CONSTITUTION.md)

*Note: This project is being developed in phases.*
