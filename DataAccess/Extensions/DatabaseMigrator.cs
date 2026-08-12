using LibraryService.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryService.DataAccess.Extensions;

public static class DatabaseMigrator
{
    public static void Migrate(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
        db.Database.Migrate();
    }
}
