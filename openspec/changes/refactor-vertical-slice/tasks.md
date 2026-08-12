## 1. Consolidate into a single project (vertical slices)

- [x] 1.1 Create `HackerRank1/Features/{Auth,Libraries,Books}` and `HackerRank1/Infrastructure/{Data,Extensions,Settings}` folders
- [x] 1.2 `git mv` controllers, services, contracts, helpers, models, DTOs, settings, context, migrations and extensions into their feature/infrastructure locations
- [x] 1.3 Delete the now-empty `BusinessLogic`, `DataAccess`, and `Entities` projects (csproj + project folders)
- [x] 1.4 Confirm no behavior change (routes, status codes, auth)

## 2. Namespace alignment to slices

- [x] 2.1 Move all types to `LibraryService.WebAPI.Features.*` / `LibraryService.WebAPI.Data` / `.Extensions` / `.Settings` per the namespace map (design D4)
- [x] 2.2 Update `Startup.cs` usings to the new namespaces
- [x] 2.3 Update EF migration designer and model snapshot entity type strings to the relocated types
- [x] 2.4 Build the solution and confirm 0 errors

## 3. Project consolidation

- [x] 3.1 Absorb EF/Npgsql/EF-Design/JWT/Newtonsoft packages into `HackerRank1.csproj`; remove all project references
- [x] 3.2 Rewrite `HackerRank1.sln` to keep only `HackerRank1` and `LibraryService.Integration.Test`
- [x] 3.3 Build the solution and confirm 0 errors

## 4. Tests and verification

- [x] 4.1 Update `IntegrationTest.cs` namespaces to the relocated types
- [x] 4.2 `dotnet build HackerRank1.sln` and `dotnet test` → all 3 tests pass

## 5. Final validation

- [x] 5.1 Confirm folder/namespace contract: every feature owns its controller, service, repository, DTO and model
- [x] 5.2 Confirm cross-feature dependencies are limited to `Book`→`Library` (FK) and the library-existence rule
- [x] 5.3 Confirm `dotnet build` has 0 errors