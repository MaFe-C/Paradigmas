using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryService.WebAPI.Features.Libraries
{
    public class LibrariesService : ILibrariesService
    {
        private readonly ILibrariesRepository _librariesRepository;

        public LibrariesService(ILibrariesRepository librariesRepository)
        {
            _librariesRepository = librariesRepository;
        }

        public async Task<IEnumerable<Library>> Get(int[]? ids)
            => await _librariesRepository.GetAsync(ids);

        public async Task<Library> Add(Library library)
            => await _librariesRepository.AddAsync(library);

        public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> projects)
            => await _librariesRepository.AddRangeAsync(projects);

        public async Task<Library> Update(Library library)
            => await _librariesRepository.UpdateAsync(library);

        public async Task<bool> Delete(Library library)
            => await _librariesRepository.DeleteAsync(library.Id);
    }

    public interface ILibrariesService
    {
        Task<IEnumerable<Library>> Get(int[]? ids);

        Task<Library> Add(Library library);

        Task<IEnumerable<Library>> AddRange(IEnumerable<Library> projects);

        Task<Library> Update(Library library);

        Task<bool> Delete(Library library);
    }
}
