## Context

The previous change (`refactor-n-layer-cleanup`) delivered dependency inversion and consistent namespaces but kept the physical N-Layers shape and left the DTOs inside the `Entities` (Domain) project. This change reshapes the solution into **Clean Architecture** without renaming projects or namespaces, by enforcing the concentric-layer contract and relocating the DTOs.

Constraints that shape this design:
- Target: .NET 8, EF Core 8, PostgreSQL (Npgsql), JWT auth, Swashbuckle.
- Keep the four projects and their root namespaces (`LibraryService.Entities.*`, `LibraryService.BusinessLogic.*`, `LibraryService.DataAccess.*`, `LibraryService.WebAPI.*`).
- The HTTP contract must not change.
- The integration tests (`WebApplicationFactory<Program>` + in-memory SQLite overriding `LibraryContext` via DI) must keep passing.

## Goals / Non-Goals

**Goals:**
- Enforce the Clean Architecture dependency rule: dependencies point inward, never outward.
- Make the layer ownership explicit in code: Domain (Entities), Application (BusinessLogic), Infrastructure (DataAccess), Presentation (HackerRank1).
- Domain contains only domain models and settings; input/output models (DTOs) live in Application.
- Green build with zero errors and unchanged integration test results.

**Non-Goals:**
- No project or namespace renames (explicitly out of scope per the branch strategy).
- No change to the HTTP contract, routes, or status codes.
- No change to auth logic (hardcoded admin/1234 remains).
- No introduction of use-case classes beyond the existing services (services already act as application use cases).
- No change to the EF/PostgreSQL provider or the DB schema.

## Decisions

### D1. Layer ownership (Clean Architecture mapping)

| Layer | Project | Root namespace | Owns |
|---|---|---|---|
| Domain | `Entities` | `LibraryService.Entities` | `Models` (Book, Library), `Settings` (JwtSettings) — zero dependencies |
| Application | `BusinessLogic` | `LibraryService.BusinessLogic` | `Services` (use cases + interfaces), `Contracts` (repository ports), `DTO` (input/output models) — depends on Domain only |
| Infrastructure | `DataAccess` | `LibraryService.DataAccess` | `Data` (LibraryContext), `Migrations`, `Repositories` (adapters implementing ports), `Extensions` (AddDataAccess, DatabaseMigrator) — depends on Application + Domain |
| Presentation | `HackerRank1` | `LibraryService.WebAPI` | Controllers, `Program`, `Startup` (composition root) — depends on Application for use cases and on Infrastructure for wiring |

The reference graph already satisfies the dependency rule from the N-Layers refactor:

```
Domain (Entities) <-- Application (BusinessLogic) <-- Infrastructure (DataAccess)
   ^                      ^                            ^
   |                      |                            |
   +------ Presentation (HackerRank1) composes all via AddDataAccess ------+
```

### D2. DTOs belong to Application, not Domain
`User`, `BookForm`, and `LibraryForm` are input/output models of the application use cases, so they move from `Entities/DTO` to `BusinessLogic/DTO` with namespace `LibraryService.BusinessLogic.DTO`. Consequences:
- `Entities.csproj` no longer needs `Newtonsoft.Json` (the DTOs were its only users); the package moves to `BusinessLogic.csproj`.
- Consumers are updated: `AuthenticationService`, `TokenGenerator`, `AuthController` (via `LibraryService.BusinessLogic.DTO`), and the integration tests (`BookForm`).
- Domain (`Entities`) keeps only the aggregate-ish domain models and configuration, with no framework references.

### D3. Ports and adapters
Repository interfaces (`ILibrariesRepository`, `IBooksRepository`) are ports owned by Application (`BusinessLogic/Contracts`). Their EF implementations in `DataAccess/Repositories` are adapters. Services (use cases) depend only on the ports, never on the concrete `DbContext`. Presentation wires the adapters through `AddDataAccess` (Infrastructure composition extension) and registers the services as scoped.

### D4. Package ownership aligned to layers
- `Entities` (Domain): no packages.
- `BusinessLogic` (Application): `System.IdentityModel.Tokens.Jwt` (token generation helper), `Newtonsoft.Json` (DTO attributes).
- `DataAccess` (Infrastructure): `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`.
- `HackerRank1` (Presentation): `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`.

## Risks / Trade-offs

- **Already-close structure**: the N-Layers branch already did most of the dependency inversion, so the visible diff of this change is small. This is expected: Clean Architecture is the natural evolution of the decoupled N-Layers, and the change's value is the explicit layer contract + DTO relocation + documentation.
- **DTO namespace change ripples to a few consumers** → Mitigation: compiler-driven update, verified by build and tests.
- **Newtonsoft.Json package relocation** → Mitigation: moved to the project that actually uses it; verified by build.

## Migration Plan

1. `git mv` the three DTOs from `Entities/DTO` to `BusinessLogic/DTO`; rename namespace to `LibraryService.BusinessLogic.DTO`.
2. Update consumers (`AuthenticationService`, `TokenGenerator`, `AuthController`, integration tests); update package references (`Entities` drops, `BusinessLogic` gains Newtonsoft.Json); build → 0 errors.
3. `dotnet build` + `dotnet test` → 3/3 green.
4. Rollback: each step is an independent commit.

## Open Questions

- None blocking. Renaming the projects to `Domain`/`Application`/`Infrastructure`/`WebAPI` was intentionally excluded to keep the branch comparable with the original project; the mapping in D1 documents the equivalent Clean layers.