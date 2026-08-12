using LibraryService.BusinessLogic.Contracts;
using LibraryService.DataAccess.Data;
using LibraryService.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.DataAccess.Repositories;

public class LibrariesRepository : ILibrariesRepository
{
    private readonly LibraryContext _context;

    public LibrariesRepository(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Library>> GetAsync(int[]? ids)
    {
        var query = _context.Libraries.AsQueryable();

        if (ids != null && ids.Any())
            query = query.Where(x => ids.Contains(x.Id));

        return await query.ToListAsync();
    }

    public async Task<Library> AddAsync(Library library)
    {
        await _context.Libraries.AddAsync(library);

        await _context.SaveChangesAsync();
        return library;
    }

    public async Task<IEnumerable<Library>> AddRangeAsync(IEnumerable<Library> libraries)
    {
        await _context.Libraries.AddRangeAsync(libraries);
        await _context.SaveChangesAsync();
        return libraries;
    }

    public async Task<Library> UpdateAsync(Library library)
    {
        var existing = await _context.Libraries.SingleAsync(x => x.Id == library.Id);
        existing.Name = library.Name;
        existing.Location = library.Location;

        _context.Libraries.Update(existing);
        await _context.SaveChangesAsync();
        return library;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var library = await _context.Libraries.FindAsync(id);
        if (library == null)
            return false;

        _context.Libraries.Remove(library);
        await _context.SaveChangesAsync();
        return true;
    }
}
