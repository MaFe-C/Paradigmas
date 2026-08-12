using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryService.WebAPI.Extensions;

public static class DatabaseMigrator
{
    public static void Migrate(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
        db.Database.Migrate();
    }
}
