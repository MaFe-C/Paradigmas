## 1. Relocate DTOs to Application

- [x] 1.1 `git mv` `Entities/DTO/{User,BookForm,LibraryForm}.cs` to `BusinessLogic/DTO/`
- [x] 1.2 Rename namespaces to `LibraryService.BusinessLogic.DTO`
- [x] 1.3 Update consumers (`AuthenticationService`, `TokenGenerator`, `AuthController`) and integration tests to the new namespace
- [x] 1.4 Build the solution and confirm 0 errors

## 2. Align package ownership with layers

- [x] 2.1 Remove `Newtonsoft.Json` from `Entities.csproj` (Domain has no packages)
- [x] 2.2 Add `Newtonsoft.Json` to `BusinessLogic.csproj` (Application owns the DTOs)
- [x] 2.3 Build the solution and confirm 0 errors

## 3. Verify Clean Architecture contract

- [x] 3.1 Confirm Domain (`Entities`) contains only `Models` + `Settings` with zero dependencies
- [x] 3.2 Confirm Application (`BusinessLogic`) owns `Services`, `Contracts`, `DTO` and depends only on Domain
- [x] 3.3 Confirm Infrastructure (`DataAccess`) implements the Application ports (adapters) and nothing depends on it except Presentation composition
- [x] 3.4 Confirm the dependency rule points inward (no references toward Presentation)
- [x] 3.5 `dotnet test` → all 3 tests pass