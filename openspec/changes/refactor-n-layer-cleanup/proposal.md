## Why

The solution has the physical skeleton of an n-layer architecture (4 projects, one-directional references) but violates the logical layering: `BusinessLogic` is coupled directly to EF Core/`LibraryContext`, `HackerRank1` (presentation) touches the `DbContext` and runs migrations itself, namespaces do not reflect layer boundaries (multiple root namespaces mixed across projects), and there is dead/incomplete code (orphan test project, `NotImplementedException`, endpoints missing while tests expect them). This makes the business layer untestable, obscures the architecture's intent, and leaves the test suite red.

## What Changes

- **Business layer decoupled from data (internal, BREAKING to internal structure)**: introduce repository interfaces (`ILibrariesRepository`, `IBooksRepository`) in `BusinessLogic`; implement them with EF Core in `DataAccess`. `BusinessLogic` drops its project reference to `DataAccess` and depends only on `Entities` + its own interfaces.
- **Presentation stops touching EF Core (internal, BREAKING to internal structure)**: `DataAccess` exposes a composition extension (`AddDataAccess`) that registers the `DbContext` and repositories. `Startup` no longer contains `DbContext`/EF code inline; it only wires abstractions. `HackerRank1` keeps a project reference to `DataAccess` for composition root only (standard .NET practice), while controllers continue to depend solely on `BusinessLogic` interfaces.
- **Namespaces aligned to layers**: rename root namespaces so each project owns a single, consistent namespace (`LibraryService.Entities.*`, `LibraryService.BusinessLogic.*`, `LibraryService.DataAccess.*`, `LibraryService.WebAPI.*`). No runtime behavior change.
- **Cleanup**: remove orphan `IntegrationTest/` project (duplicate, net6.0, not in the .sln), remove `MSTest.TestFramework` from the web project, remove the `Microsoft.Extensions.Configuration.Json` v11-preview package, and align test packages to net8.0 versions.
- **Restore intended functionality required by existing tests**: implement `BooksService.Add/Update/Delete`, `LibrariesService.Delete`, and the missing endpoints (`POST/PUT/DELETE api/libraries/{libraryId}/books`, `DELETE api/libraries/{libraryId}`) so the integration tests pass and no endpoint regresses.

## Capabilities

### New Capabilities
- `library-api`: HTTP behavior of the Library Service — CRUD for libraries and books (routes, status codes, authorization), which is what the integration tests validate. No specs exist yet, so this change introduces the first capability spec describing the intended API surface.

### Modified Capabilities
- None. There are no existing specs under `openspec/specs/`.

## Impact

- **Projects**: `BusinessLogic` (interfaces + services), `DataAccess` (repositories + DI extension, new reference to `BusinessLogic`), `HackerRank1` (composition via extension, namespace renames, package cleanup), `Entities` (namespace renames only).
- **Tests**: `LibraryService.Integration.Test` updated for namespace renames and to verify the completed CRUD endpoints; orphan `IntegrationTest/` removed.
- **Dependencies**: web project drops EF/Npgsql/preview-config/`MSTest` packages (EF/Npgsql remain in `DataAccess`); test packages bumped to 8.0.x.
- **No change** to the HTTP contract shape currently served (entities are returned as-is; DTO mapping is explicitly out of scope to avoid changing the API contract).
