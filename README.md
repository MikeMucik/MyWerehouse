# MyWarehouse

Warehouse Management System REST API built with ASP.NET Core, CQRS and Domain-Driven Design.

The project models real warehouse processes such as goods receipt, inventory management, picking and shipment.

The project currently uses a layered architecture and is gradually being refactored towards Clean Architecture.

## Links

- Repository
https://github.com/MikeMucik/MyWerehouse
- Swagger UI (Azure)
https://mywarehouse-api-hiermet-ffggb2dwe5crh4gq.polandcentral-01.azurewebsites.net/swagger.html
- CI Pipeline (GitHub Actions)
  https://github.com/MikeMucik/MyWerehouse/actions

## Features

✔ Goods Receipts

✔ Inventory Management

✔ Picking

✔ Reverse Picking

✔ Warehouse Locations

✔ Shipment Management

✔ Product Allocation

✔ History Tracking

## Screenshot

![Swagger UI](docs/images/swagger.png)

## Architecture

The application currently follows a layered architecture:

Presentation
    ↓
Application
    ↓
Domain
    ↓
Infrastructure

## Solution Structure

├─ MyWarehouse.Api
│  └─ Controllers, Swagger, Middleware
├─ MyWarehouse.Application
│  └─ Commands, Queries, Handlers, Results
├─ MyWarehouse.Domain
│  └─ Entities, ValueObjects, Domain Logic
├─ MyWarehouse.Infrastructure
│  └─ DbContext, Repositories, EF Core Config
└─ tests/
   └─ MyWarehouse.IntegrationTests

The current solution follows a layered architecture. The long-term goal is to complete the migration to Clean Architecture by further reducing dependencies between layers.

## Technologies

- .NET 9

- ASP.NET Core Web API

- MediatR (CQRS)

- EF Core

- SQL Server

- Swagger / OpenAPI

## Testing

The project currently contains over 300 integration tests covering warehouse business scenarios.

Testing technologies:

- SQLite InMemory
- EF Core InMemory
- GitHub Actions

## Running locally
dotnet restore

dotnet run

dotnet test
## Domain

The warehouse domain is based on the following business concepts:

Issue – an order to issue goods (e.g. created based on an e-mail from a customer)

Receipt - receipt of goods, a set of pallets received in one delivery

Pallet – a physical warehouse pallet with a specific number of boxes

Picking – the process of assembling goods for a specific issue

ReversePicking - the process of disassembly when the picking is completed but the Issue order is withdrawn

BestBefore - expiration date, taken into account when allocating pallets

## Future Improvements

- Complete migration to Clean Architecture

- JWT authentication and authorization

- Docker support

- Improved pallet allocation policies

- Warehouse workload planning