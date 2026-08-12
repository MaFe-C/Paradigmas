## Why

The N-Layers refactor already introduced dependency inversion (repository ports owned by the business layer, a `DataAccess` composition extension, and aligned namespaces). This change reorganizes the same solution under **Clean Architecture**: concentric layers with the dependency rule pointing strictly inward. Project and namespace names are kept as-is (`Entities`, `BusinessLogic`, `DataAccess`, `HackerRank1`), and the only structural change is moving the input/output models (DTOs) out of the Domain layer into the Application layer, where they belong.

## What Changes

- **DTOs move from Domain to Application**: `User`, `BookForm`, and `LibraryForm` move from `Entities/DTO` to `BusinessLogic/DTO` (namespace `LibraryService.BusinessLogic.DTO`). Domain now contains only domain models and settings.
- **Layer mapping (documented, not renamed)**:
  - **Domain** = `Entities` → `Models` (Book, Library) + `Settings` (JwtSettings). Zero dependencies.
  - **Application** = `BusinessLogic` → `Services` (use cases), `Contracts` (ports: repository interfaces), `DTO` (input/output models). Depends only on Domain.
  - **Infrastructure** = `DataAccess` → `LibraryContext`, `Migrations`, `Repositories` (adapters implementing the ports), `Extensions` (composition). Depends on Application + Domain.
  - **Presentation** = `HackerRank1` → controllers, `Program`, `Startup` (composition root). Depends on Application for use cases and on Infrastructure for wiring.
- **Dependency rule holds inward**: nothing points toward `HackerRank1`; `BusinessLogic` no longer touches `DataAccess`; the EF/`DbContext` knowledge lives only in `DataAccess` and is composed by the presentation layer through `AddDataAccess`.
- **No behavior change**: HTTP contract, endpoints, status codes, auth rules, and tests remain identical (tests updated only in namespace usings).

## Capabilities

### New Capabilities
- None. The `library-api` capability spec already exists and is architecture-agnostic; it is carried over unchanged.

### Modified Capabilities
- None.

## Impact

- **Projects**: `Entities` drops the `Newtonsoft.Json` package (DTOs left the project); `BusinessLogic` gains it. No other project/reference changes.
- **Tests**: `LibraryService.Integration.Test` updated only in namespace usings for the relocated DTOs.
- **Dependencies**: package ownership realigned to the layers; no version changes.
- **No change** to the HTTP contract, endpoints, status codes, or auth behavior.