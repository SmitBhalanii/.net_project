# Architecture

The architecture of RentKart is a Clean Layered Architecture based on ASP.NET Core MVC.

## Layers

```text
RentKart.sln
├── RentKart.Web (Presentation Layer)
├── RentKart.Infrastructure (Data Access, Implementations)
├── RentKart.Core (Domain Entities, Interfaces)
└── RentKart.Tests (Unit & Integration Tests)
```

## Dependency Direction
```text
Web
 ↓
Infrastructure
 ↓
Core
```
**Core** is completely independent of Web, EF Core, SQL, or other technical implementations.

## Key Principles
1. **Thin Controllers:** Controllers only receive requests, bind models, check authorization, call services, and return views. No business logic.
2. **Service Layer:** All business rules reside in services injected via interfaces (e.g. `IBookingService`).
3. **Database Access:** Entity Framework Core using `ApplicationDbContext` combined with the Service layer is the default pattern. Do not create unnecessary repository abstractions.
4. **ViewModels:** Domain entities are not passed directly to Razor views. Use ViewModels for structured data flow.
5. **Entity Design:** Entities only contain domain state and relationships. No UI properties (e.g. SelectLists, HTML strings).
6. **Error Handling:** Centralized. Never expose internal stack traces or connection strings to users.
7. **Identity:** ASP.NET Core Identity handles authentication. Custom role-based authorization restricts access to specific dashboard pages.
