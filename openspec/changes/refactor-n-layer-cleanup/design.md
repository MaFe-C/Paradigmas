## Context

The current solution is a physical n-layer skeleton with logical leaks: `BusinessLogic` depends directly on `DataAccess` and injects `LibraryContext` (EF Core) into services; `HackerRank1` (presentation) registers and uses the `DbContext` and runs migrations itself; namespaces are mixed across three roots (`HackerRank1.*`, `LibraryService.WebAPI.*`); and the tests expect CRUD endpoints that are missing. See proposal.md - Why.

Constraints that shape this design:
- Target: .NET 8, EF Core 8, PostgreSQL (Npgsql), JWT auth, Swashbuckle.
- Existing tests use `WebApplicationFactory` + in-memory SQLite and override `LibraryContext` via DI.
- The HTTP contract must not change (entities serialized as-is; DTO mapping is out of scope).

## Goals / Non-Goals

**Goals:**
- Decouple `BusinessLogic` from data access through repository interfaces it owns.
- Remove EF/`DbContext` knowledge from the presentation layer.
- Single consistent root namespace per project.
- Restore the missing CRUD endpoints so the documented behavior (and tests) work.
- Green build with zero warnings introduced; integration tests passing.

**Non-Goals:**
- No DTO mapping at the API boundary (contract shape stays identical).
- No change to auth logic (hardcoded admin/1234 remains; only its placement/layer is kept).
- No introduction of a separate solution-wide "Contracts" project (interfaces live in `BusinessLogic`).
- No change to the EF/PostgreSQL provider or the existing migration schema.

## Decisions

### D1. Repository interfaces owned by BusinessLogic (Dependency Inversion)
`BusinessLogic` defines `ILibrariesRepository` and `IBooksRepository` (in a `Contracts` folder). `DataAccess` implements them. `BusinessLogic` drops its reference to `DataAccess`; `DataAccess` now references `BusinessLogic` for the interfaces. New reference graph:

```
Entities (sin deps)
    ▲            ▲
    │            │
BusinessLogic ───┘   (Services + I*Repository; NO referencia a DataAccess)
    ▲            ▲
    │            │
DataAccess        │   (LibraryContext, Migrations, Repositories, AddDataAccess)
    │            │
    │            └──► reference a BusinessLogic (interfaces)
    │
HackerRank1 ──► BusinessLogic + Entities
    └──────► DataAccess (SÓLO para composición: AddDataAccess extension)
```

**Alternatives considered:** putting interfaces in a new `Contracts` project (rejected: extra project for two interfaces; keeps surface small); letting services keep `LibraryContext` (rejected: defeats testability, the core leak).

Repository signatures follow the current service methods (int[]? ids, single save per op) so behavior stays equivalent.

### D2. Composition via `AddDataAccess` extension in DataAccess
`DataAccess` exposes `IServiceCollection.AddDataAccess(this, IConfiguration)` that registers the `DbContext` pool and the repositories (scoped), plus `DatabaseMigrator.Migrate(IServiceProvider)` for the startup migration. `HackerRank1` keeps a project reference to `DataAccess` **only** to call these — the standard .NET composition-root exception. `Startup` no longer contains EF code; it wires `AddDataAccess` and business services.

Lifetimes: repositories and services registered **Scoped** (previously transient), matching the scoped `DbContext`. **Alternative:** leaving services Transient — rejected; a transient service capturing a scoped `DbContext` is a lifetime anti-pattern.

### D3. Single root namespace per project
Map each type to its project so namespaces encode the layer:

| Project | Current namespace | New namespace |
|---|---|---|
| Entities | `LibraryService.WebAPI.Data` / `HackerRank1.Entities` / `HackerRank1.DTO` | `LibraryService.Entities.Models` (Library, Book), `.Settings` (JwtSettings), `.DTO` (User, LibraryForm, BookForm) |
| BusinessLogic | `LibraryService.WebAPI.Services` / `HackerRank1.Services` / `HackerRank1.Helpers` | `LibraryService.BusinessLogic.Services` (services + IAuthenticationService), `.Contracts` (I*Repository), `.Helpers` (TokenGenerator) |
| DataAccess | `LibraryService.WebAPI.Data` / `HackerRank1.Migrations` | `LibraryService.DataAccess.Data` (LibraryContext), `.Repositories`, `.Extensions` (AddDataAccess, DatabaseMigrator), `.Migrations` |
| HackerRank1 | `HackerRank1.*` | `LibraryService.WebAPI.Controllers`, `.Program/Startup` |

Migration files (`.Designer.cs`, `ModelSnapshot`) must update the namespace AND their `[DbContext(typeof(LibraryContext))]` usings to the renamed context namespace. The `[Migration]` id attribute is unaffected.

### D4. Missing endpoints restore existing intended behavior
Complete `BooksService.Add/Update/Delete`, `LibrariesService.Delete`, and add the missing routes (`POST/PUT/DELETE api/libraries/{libraryId}/books`, `DELETE api/libraries/{libraryId}`). `Get` on books and delete-on-missing checks return `404` when the parent library doesn't exist (library existence is a business rule checked by the service through `ILibrariesRepository`). Deleting a library relies on the existing cascade FK to remove books.

Books endpoints keep `[Authorize]` (existing behavior); the integration tests are updated to obtain a token via `POST /login` (admin/1234) and attach it, so security is not weakened.

### D5. Cleanup and version alignment
- Delete orphan `IntegrationTest/` project (net6.0 duplicate, not in the .sln).
- Web: remove `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.Extensions.Configuration.Json` (v11 preview, net8-incompatible), and `MSTest.TestFramework`. EF/Npgsql remain in `DataAccess`.
- Tests: bump `Microsoft.AspNetCore.Mvc.Testing`, EF InMemory/Sqlite, and `FluentAssertions` to net8-era 8.0.x versions.

## Risks / Trade-offs

- **Namespace renames touch every file** → [Risk] missed reference causes build errors → Mitigation: compiler-driven rename (build between steps), keep behavior identical so only namespaces move.
- **Repository refactor can subtly change query behavior** → Mitigation: services delegate to repositories 1:1 mirroring current LINQ (single `SaveChanges` per op, same filters); integration tests guard the contract.
- **Test DI override of `LibraryContext`** with repository pattern → Mitigation: repositories resolve `LibraryContext` from the container, so the existing `RemoveAll`/`AddSingleton` override in tests still works.
- **Moving `Migrate()` out of Startup** → Mitigation: `DatabaseMigrator.Migrate` keeps the same order (migrate before `UseRouting`); startup behavior unchanged.
- **Book endpoints require auth** → Mitigation: tests authenticate first; spec (library-api) already documents the 401 behavior.

## Migration Plan

1. Namespace renames across Entities/BusinessLogic/DataAccess/Web (+ Designer/Snapshot), build.
2. Introduce repository interfaces + implementations; rewire services; flip project references; move DbContext/migration registration into `AddDataAccess`/`DatabaseMigrator`; build.
3. Cleanup packages and orphan project; build.
4. Implement missing CRUD in services/controllers; update tests (namespaces + auth flow + package versions); run full build and test suite.
5. Rollback: each step is an independent commit; reversion of any step restores the previous green state.

## Open Questions

- None blocking. Whether to later map entities to DTOs at the API boundary is deferred (would change the contract and is intentionally a non-goal here).
