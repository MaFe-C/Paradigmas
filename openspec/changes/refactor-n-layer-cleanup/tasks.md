## 1. Namespace alignment

- [x] 1.1 Rename Entities namespaces: `Models/Library.cs` and `Models/Book.cs` to `LibraryService.Entities.Models`; `DTO/User.cs`, `DTO/LibraryForm.cs`, `DTO/BookForm.cs` to `LibraryService.Entities.DTO`; `Settings/JwtSettings.cs` to `LibraryService.Entities.Settings`
- [x] 1.2 Rename BusinessLogic namespaces: `Services/*.cs` to `LibraryService.BusinessLogic.Services`; `Helpers/TokenGenerator.cs` to `LibraryService.BusinessLogic.Helpers`
- [x] 1.3 Rename DataAccess namespaces: `Data/LibraryContext.cs` to `LibraryService.DataAccess.Data`; migration files (`InitialCreate.cs`, `.Designer.cs`, `ModelSnapshot.cs`) to `LibraryService.DataAccess.Migrations`, updating their `[DbContext(typeof(LibraryContext))]` usings
- [x] 1.4 Rename HackerRank1 namespaces: controllers to `LibraryService.WebAPI.Controllers`; `Program.cs`/`Startup.cs` to `LibraryService.WebAPI`; remove now-unused `HackerRank1.*` using directives
- [x] 1.5 Update `LibraryService.Integration.Test` and remove the orphan `IntegrationTest/` project directory
- [x] 1.6 Build the solution and confirm 0 errors

## 2. Decouple BusinessLogic from DataAccess

- [x] 2.1 Add `ILibrariesRepository` and `IBooksRepository` under `BusinessLogic/Contracts` (methods mirroring current service queries: get-by-ids, add, update, delete; `Delete` takes the entity id)
- [x] 2.2 Implement `LibrariesRepository` and `BooksRepository` in `DataAccess/Repositories` using `LibraryContext`, preserving current LINQ filters and single `SaveChanges` per operation
- [x] 2.3 Rewire `LibrariesService` and `BooksService` to depend on the repository interfaces instead of `LibraryContext`; implement library-existence checks (business rule) for book add/update/delete and library delete
- [x] 2.4 Remove the `DataAccess` ProjectReference from `BusinessLogic.csproj`; add a ProjectReference to `BusinessLogic` from `DataAccess.csproj`
- [x] 2.5 Build the solution and confirm 0 errors

## 3. Remove EF/DbContext from the presentation layer

- [x] 3.1 Add `AddDataAccess(this IServiceCollection, IConfiguration)` extension in `DataAccess/Extensions` registering `AddDbContextPool<LibraryContext>` (Npgsql, retry options, poolSize 20) and the repositories as scoped
- [x] 3.2 Add `DatabaseMigrator.Migrate(IServiceProvider)` helper in `DataAccess/Extensions` (runs `db.Database.Migrate()`)
- [x] 3.3 Update `Startup.cs` to call `AddDataAccess(Configuration)`, register business services as scoped, and invoke the migrator before `UseRouting`; remove all `Microsoft.EntityFrameworkCore` / Npgsql using directives from the web project
- [x] 3.4 Remove EF, Npgsql, `Microsoft.Extensions.Configuration.Json`, and `MSTest.TestFramework` packages from `HackerRank1.csproj`
- [x] 3.5 Build the solution and confirm 0 errors

## 4. Restore intended functionality (missing CRUD)

- [x] 4.1 Implement `BooksService.Add/Update/Delete` and `LibrariesService.Delete` (delegating to repositories; return `false`/null on missing parent) — replace `NotImplementedException`
- [x] 4.2 Add `POST`, `PUT`, and `DELETE` endpoints to `BooksController` (201 on create, 204 on update/delete, 404 when library/book missing) keeping `[Authorize]`
- [x] 4.3 Add `DELETE` endpoint to `LibrariesController` (204 on success, 404 when missing)
- [x] 4.4 Build the solution and confirm 0 errors

## 5. Test alignment and verification

- [ ] 5.1 Bump test packages in `LibraryService.Integration.Test.csproj` to net8-era versions (`Microsoft.AspNetCore.Mvc.Testing` 8.0.x, EF InMemory/Sqlite 8.0.x, `FluentAssertions` 6.x)
- [ ] 5.2 Update `IntegrationTest.cs` namespaces for the renamed types and add a login helper that obtains a JWT via `POST /login` (admin/1234) and attaches it to book requests
- [ ] 5.3 Run `dotnet build HackerRank1.sln` and `dotnet test` and confirm the full suite passes with no new warnings

## 6. Final validation

- [ ] 6.1 Confirm the dependency graph matches design D1 (BusinessLogic no longer references DataAccess; DataAccess references BusinessLogic; Web references all three for composition only)
- [ ] 6.2 Confirm `dotnet build` produces 0 errors and the project has no orphan/misplaced packages
