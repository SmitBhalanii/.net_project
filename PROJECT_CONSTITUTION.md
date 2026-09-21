# RENTKART — MASTER PROJECT CONSTITUTION & DEVELOPMENT RULEBOOK

## 1. PROJECT IDENTITY
* **Project Name:** RentKart
* **Tagline:** Rent Anything You Need.
* **Project Type:** Local Rental Marketplace
* **Primary model:** Business → Customer
* **Platform role:** Admin
* **Roles:** Customer, Business, Admin (NO Staff role)

## 2. BUSINESS CONCEPT
RentKart connects customers with nearby rental businesses. Customers can discover, check availability, request bookings, visit the location, use equipment, return it, and review the experience. Businesses can manage their profile, equipment, inventory, bookings, and damage reports.

## 3. CRITICAL BUSINESS RULE — NO DELIVERY
The current RentKart version does NOT provide home delivery. No delivery fleet, tracking, routes, or charges. The customer physically visits the business/pickup location.

## 4. CURRENT TECHNOLOGY STACK
* C#
* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* Razor Views
* Bootstrap 5
* HTML5 / CSS3 / JavaScript
* No unnecessary frameworks.

## 5. ARCHITECTURE
Clean layered architecture with the following projects:
* `RentKart.Core`: Domain-level code (Entities, Enums, DTOs, Interfaces, Constants, Exceptions)
* `RentKart.Infrastructure`: Technical implementation (Data, Configurations, Repositories, Services, Migrations, Identity)
* `RentKart.Web`: Presentation/entry point (Controllers, ViewModels, Views)
* `RentKart.Tests`: Unit, Integration, Controller, and Service Tests

## 6. DEPENDENCY DIRECTION
`Web` -> `Infrastructure` -> `Core`
Core must remain independent.

## 7. CONTROLLER RULE
Controllers must remain thin, delegating business logic to services.

## 8. SERVICE RULE
Business logic belongs in services using interfaces (e.g., `IEquipmentService` / `EquipmentService`). One consistent pattern must be used throughout.

## 9. DATABASE ACCESS RULE
Use EF Core centralized through `ApplicationDbContext`. Service + EF Core DbContext is the default pattern. Avoid unnecessary repository abstractions unless architecturally justified.

## 10. ENTITY RULES
Entities belong in `RentKart.Core/Entities`. Must use meaningful names, correct types, appropriate nullable reference types. NO UI-specific properties (HTML, ViewData).

## 11. NAMING CONVENTION
* Classes, Methods, Properties, Async methods: `PascalCase`
* Private fields: `_camelCase`
* Interfaces: Prefix with `I` (e.g., `IEquipmentService`)
* Boolean properties: Prefix with `Is` or `Has` (e.g., `IsActive`)

## 12. ASYNC PROGRAMMING
Database operations must use async methods (`ToListAsync()`, etc.). Use `async Task` and `async Task<T>` consistently.

## 13. DEPENDENCY INJECTION
Use ASP.NET Core built-in DI via constructors. Do not manually instantiate services with `new` inside controllers.

## 14. VIEWMODEL RULE
Use ViewModels for UI communication instead of passing Domain Entities directly. Examples: `EquipmentCreateViewModel`, `BookingCreateViewModel`.

## 15. VALIDATION
Server-side validation using DataAnnotations for all important forms. Client-side validation enabled as secondary measure.

## 16. ERROR HANDLING
Centralized error handling. Expected business errors via services/results. Do not expose stack traces, SQL errors, or secrets to end users.

## 17. AUTHENTICATION
Use ASP.NET Core Identity. No custom password authentication or tables.

## 18. AUTHORIZATION
Role-based authorization (Admin, Business, Customer) using `[Authorize(Roles = "...")]`.

## 19. ROLE REDIRECTION
Redirect users to their respective dashboards post-login (Admin Dashboard, Business Dashboard, Customer Dashboard).

## 20. IDENTITY USER DESIGN
Central user is `ApplicationUser : IdentityUser`. Do not create separate authentication systems for different roles.

## 21. SECURITY RULES
Never hardcode secrets. Use appsettings and user secrets. Enforce anti-forgery, authorization, input validation, and secure file uploads.

## 22. FILE UPLOAD RULE
Validate extension, MIME type, size, and safety. Store with generated safe filenames.

## 23. ENUM RULE
Use Enums for business states instead of arbitrary strings (e.g., `BookingStatus`, `EquipmentStatus`).

## 24. DATABASE CONFIGURATION
Use EF Core Fluent API with separate configuration classes in `Infrastructure/Data/Configurations`.

## 25. DATABASE MIGRATIONS
Every schema change requires EF Core migrations. Do not manually modify the database. Meaningful migration names required.

## 26. CONTROLLER NAMING
Consistent naming (e.g., `HomeController`, `AdminController`, `BusinessController`).

## 27. VIEW STRUCTURE
Organized by controller in `Views`. Use strongly typed Razor views. Avoid excessive ViewBag/ViewData.

## 28. UI CONSISTENCY
One design system (Bootstrap 5) with consistent styling for spacing, buttons, forms, tables, and badges across the entire app.

## 29. DASHBOARD CONSISTENCY
All dashboards (Admin, Business, Customer) must follow the same structure and visual language.

## 30. BUSINESS MODEL FLOW
* **Customer:** Discovers equipment -> requests booking -> pickup -> rental -> return -> review.
* **Business:** Lists equipment -> receives booking -> approves -> issues -> receives -> inspects.
* **Admin:** Approves businesses -> manages categories -> monitors marketplace.

## 31. CORE ENTITIES (Future phases)
`ApplicationUser`, `Business`, `Category`, `Equipment`, `EquipmentImage`, `Booking`, `BookingItem`, `Review`, `Wishlist`, `Notification`, `DamageReport`, `MaintenanceRecord`, `RentalAgreement`, `Invoice`, `AuditLog`.

## 32. IMPORTANT FUTURE FEATURES (DO NOT IMPLEMENT NOW)
Online payments, delivery, P2P marketplace, AI features, dynamic pricing, advanced maps, chat, KYC, subscriptions.

## 33. CODING STYLE RULE
CRITICAL: Everyone uses the exact same patterns, naming, handling, and logic structures across the codebase.

## 34. DO NOT DUPLICATE CODE
Reuse, extend, or refactor existing logic instead of rewriting.

## 35. AGENT WORKFLOW
Inspect -> Understand -> Identify -> Plan -> Implement -> Build -> Fix -> Test -> Verify -> Review -> Summarize -> Stop.

## 36. BEFORE MODIFYING EXISTING CODE
Always inspect related codebase (entities, services, controllers, viewmodels, views, context) before overwriting.

## 37. BUILD REQUIREMENT
The solution MUST compile (`dotnet build`) at the end of every phase.

## 38. DATABASE REQUIREMENT
Schema changes require Migration creation and application.

## 39. TESTING REQUIREMENT
Important features require basic validation. Services need unit tests where practical.

## 40. SEED DATA
Must be deterministic and safe, implemented in a dedicated phase.

## 41. GIT RULE
Use meaningful commit messages following conventional commits (`feat:`, `fix:`, `refactor:`).

## 42. DOCUMENTATION
Keep `README.md`, `ARCHITECTURE.md`, `DATABASE.md` up to date to reflect reality.

## 43. DEVELOPMENT PHASE RULE
Implement ONLY the requested phase. Stop and await further instructions.

## 44. PHASE COMPLETION REPORT
End every phase with a standardized report covering files modified, DB changes, features built, build status, test steps, and dependencies.
