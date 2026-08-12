## Context

The previous change (`refactor-n-layer-cleanup`) delivered a clean N-Layers structure with dependency inversion. This change reorganizes the same solution as a **vertical slice**: one project, code grouped by feature, each feature self-contained from API to persistence.

Constraints that shape this design:
- Target: .NET 8, EF Core 8, PostgreSQL (Npgsql), JWT auth, Swashbuckle.
- The HTTP contract must not change (routes, status codes, entity-shaped responses).
- The integration tests (`WebApplicationFactory<Program>` + in-memory SQLite overriding `LibraryContext` via DI) must keep passing.
- A single solution (`HackerRank1.sln`) with the web project and the test project.

## Goals / Non-Goals

**Goals:**
- One project (`HackerRank1`) whose code is organized by functional feature (`Features/`).
- Each feature owns its controller, service (with interface), repository (with interface), DTO, and domain model.
- Cross-cutting technical concerns centralized in `Infrastructure/`.
- Single root namespace `LibraryService.WebAPI` with sub-namespaces per feature.
- Green build with zero errors and unchanged integration test results.

**Non-Goals:**
- No change to the HTTP contract, routes, or status codes.
- No change to auth logic (hardcoded admin/1234 remains).
- No change to the EF/PostgreSQL provider or the DB schema.
- No MediatR/handler-style decomposition; the vertical slices use the existing service pattern to stay comparable with the other branches.
- No new packages.

## Decisions

### D1. Single project, single composition root
All code moves into `HackerRank1` (namespace root `LibraryService.WebAPI`). `HackerRank1.csproj` absorbs the packages previously spread across projects:
- EF Core 8.0.2, Npgsql 8.0.2, EF Design 8.0.2 (from `DataAccess`)
- `System.IdentityModel.Tokens.Jwt` 7.1.2 (from `BusinessLogic`)
- Newtonsoft.Json 13.0.3 (from `Entities`)
- Existing web packages (JwtBearer 8.0.16, Swashbuckle 6.5.0)

`Startup` remains the composition root: it registers JWT, services, and `AddDataAccess` (now in `LibraryService.WebAPI.Extensions`). The `AddDataAccess` extension and `DatabaseMigrator` are kept as-is, only relocated and re-namespaced, so the test DI override of `LibraryContext` keeps working.

### D2. Feature slices own their layers
Each folder under `Features/` is a complete vertical slice:

| Feature | Presentation | Business | Data | Types owned |
|---|---|---|---|---|
| `Auth` | `AuthController` | `AuthenticationService`/`IAuthenticationService` | (uses `TokenGenerator`) | `User` (DTO), `TokenResponse` |
| `Libraries` | `LibrariesController` | `LibrariesService`/`ILibrariesService` | `LibrariesRepository`/`ILibrariesRepository` | `Library` (model), `LibraryForm` (DTO) |
| `Books` | `BooksController` | `BooksService`/`IBooksService` | `BooksRepository`/`IBooksRepository` | `Book` (model), `BookForm` (DTO) |

Each slice is a namespace `LibraryService.WebAPI.Features.<Name>` so the folder, namespace, and feature coincide. Cross-slice references are minimal and explicit: `BooksService` depends on `ILibrariesRepository` (Libraries) to enforce the "library must exist" business rule, and `Book` references `Library` only via its navigation property. `BooksController` uses `ILibrariesService` to 404 when the parent library is missing.

### D3. Shared infrastructure separated from features
`Infrastructure/` holds concerns that are not a user feature:
- `Data/LibraryContext` + `Migrations/` (namespace `LibraryService.WebAPI.Data` / `.Data.Migrations`)
- `Extensions/DataAccessServiceCollectionExtensions` + `Extensions/DatabaseMigrator` (namespace `LibraryService.WebAPI.Extensions`)
- `Settings/JwtSettings` (namespace `LibraryService.WebAPI.Settings`)

The EF model strings inside the migration designer/snapshot are updated to the new entity type names (`LibraryService.WebAPI.Features.Books.Book`, `LibraryService.WebAPI.Features.Libraries.Library`) so the stored model matches the relocated types.

### D4. Namespace map

| Old namespace | New namespace |
|---|---|
| `LibraryService.WebAPI.Controllers` (Auth/Books/Libraries controllers) | `LibraryService.WebAPI.Features.{Auth,Books,Libraries}` |
| `LibraryService.BusinessLogic.Services` | `LibraryService.WebAPI.Features.{Auth,Books,Libraries}` |
| `LibraryService.BusinessLogic.Contracts` | `LibraryService.WebAPI.Features.{Books,Libraries}` |
| `LibraryService.BusinessLogic.Helpers` (TokenGenerator) | `LibraryService.WebAPI.Features.Auth` |
| `LibraryService.Entities.Models` | `LibraryService.WebAPI.Features.{Books,Libraries}` |
| `LibraryService.Entities.DTO` | `LibraryService.WebAPI.Features.{Auth,Books,Libraries}` |
| `LibraryService.Entities.Settings` | `LibraryService.WebAPI.Settings` |
| `LibraryService.DataAccess.Data` | `LibraryService.WebAPI.Data` |
| `LibraryService.DataAccess.Migrations` | `LibraryService.WebAPI.Data.Migrations` |
| `LibraryService.DataAccess.Repositories` | `LibraryService.WebAPI.Features.{Books,Libraries}` |
| `LibraryService.DataAccess.Extensions` | `LibraryService.WebAPI.Extensions` |

## Risks / Trade-offs

- **Single project couples features at compile time** → Acceptable for this scope; the folder/namespace contract encodes the slice boundaries and dependency direction.
- **Namespace renames touch many files** → Mitigation: mechanical rename driven by the compiler (build between steps), behavior unchanged.
- **Migration model strings** → Mitigation: updated to the relocated entity type names; verified by the SQLite integration tests that run the migrations.
- **Book ↔ Library cross-feature dependency** → Mitigation: kept to the FK navigation and the library-existence rule only, documented in D2.

## Migration Plan

1. Create `Features/` and `Infrastructure/` folders in `HackerRank1`; `git mv` every file from `BusinessLogic`, `DataAccess`, `Entities`, and the web `Controllers` into its feature/infrastructure location; delete the empty projects.
2. Update namespaces/usings across all moved files; update `Startup` and the migration designer/snapshot entity strings; build → 0 errors.
3. Consolidate `HackerRank1.csproj` (packages in, references out) and rewrite `HackerRank1.sln` (only web + test); build → 0 errors.
4. Update test namespaces; `dotnet build` + `dotnet test` → 3/3 green.
5. Rollback: each step is an independent commit.

## Open Questions

- None blocking. Whether each slice should later adopt handler-based endpoints (e.g., a `Handlers` sub-folder per feature) is deferred; the current service-based slices are intentionally comparable with the N-Layers and Clean branches.