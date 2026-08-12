## Why

The solution already implements a solid N-Layers architecture (projects `Entities`, `BusinessLogic`, `DataAccess`, `HackerRank1`) with dependency inversion (repositories and DI extension). This change reorganizes the same behavior under **vertical slice architecture**: instead of organizing code by technical layer, each functional feature owns its complete stack (endpoint, business logic, persistence) in a single project with `Features/` folders. This is the canonical structure of a vertical slice: high cohesion per feature, low coupling between slices, and a single composition root.

## What Changes

- **One project instead of four**: the `BusinessLogic`, `DataAccess`, and `Entities` projects are folded into a single web project (`HackerRank1`). The `.sln` now contains only `HackerRank1` and `LibraryService.Integration.Test`.
- **Feature folders own their slices**: each feature folder contains its controller (presentation), service + interface (business logic), repository + interface (data access), and its own DTO/domain model:
  - `Features/Auth` → login, `User` DTO, `AuthenticationService`, `TokenGenerator`.
  - `Features/Libraries` → library CRUD, `Library` model, `LibraryForm`, `LibrariesService`, `LibrariesRepository`.
  - `Features/Books` → book CRUD, `Book` model, `BookForm`, `BooksService`, `BooksRepository`.
- **Shared infrastructure** stays centralized in `Infrastructure/` (cross-cutting, not a feature): `Data/LibraryContext` + `Migrations`, `Extensions/` (`AddDataAccess`, `DatabaseMigrator`), `Settings/JwtSettings`.
- **Namespaces mirror the slices**: single root `LibraryService.WebAPI` with `LibraryService.WebAPI.Features.{Auth|Libraries|Books}` and `LibraryService.WebAPI.Infrastructure.*` sub-namespaces.
- **Allowed cross-feature reference**: `Book` (Books) references `Library` (Libraries) only through the foreign-key navigation property, a pragmatic minimal coupling between slices.
- **No behavior change**: HTTP contract, auth rules, endpoints, and tests remain identical (the tests were updated only in namespace/usings).

## Capabilities

### New Capabilities
- None. The `library-api` capability spec already exists from the previous change; it is carried over unchanged because the HTTP behavior does not change.

### Modified Capabilities
- None (spec is architecture-agnostic).

## Impact

- **Projects**: removed `BusinessLogic`, `DataAccess`, `Entities` projects (folded into `HackerRank1`). `HackerRank1.csproj` absorbs the EF/Npgsql/Newtonsoft/JWT packages and drops all project references.
- **Tests**: `LibraryService.Integration.Test` updated only in namespace usings for the relocated types.
- **Dependencies**: no package version changes; only package locations change (now in a single project).
- **No change** to the HTTP contract, endpoints, status codes, or auth behavior.