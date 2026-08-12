using LibraryService.WebAPI.Data;
using LibraryService.WebAPI.Features.Libraries;
using LibraryService.WebAPI.Features.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryService.WebAPI.Extensions;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<LibraryContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 1,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }),
            poolSize: 20);

        services.AddScoped<ILibrariesRepository, LibrariesRepository>();
        services.AddScoped<IBooksRepository, BooksRepository>();

        return services;
    }
}
