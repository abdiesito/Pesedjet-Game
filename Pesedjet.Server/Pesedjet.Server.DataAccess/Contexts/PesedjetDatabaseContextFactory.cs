using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Pesedjet.Server.DataAccess.Contexts;

public class PesedjetDatabaseContextFactory : IDesignTimeDbContextFactory<PesedjetDatabaseContext>
{
    public PesedjetDatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<PesedjetDatabaseContextFactory>()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "ConnectionStrings:DefaultConnection not found. Set it with: " +
                                   "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\" --project Pesedjet.Server");

        var optionsBuilder = new DbContextOptionsBuilder<PesedjetDatabaseContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PesedjetDatabaseContext(optionsBuilder.Options);
    }
}