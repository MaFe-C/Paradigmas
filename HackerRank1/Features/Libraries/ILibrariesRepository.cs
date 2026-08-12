namespace LibraryService.WebAPI.Features.Libraries;

public interface ILibrariesRepository
{
    Task<IEnumerable<Library>> GetAsync(int[]? ids);

    Task<Library> AddAsync(Library library);

    Task<IEnumerable<Library>> AddRangeAsync(IEnumerable<Library> libraries);

    Task<Library> UpdateAsync(Library library);

    Task<bool> DeleteAsync(int id);
}
