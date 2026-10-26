using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Pesedjet.Server.DataAccess.Contexts;

public class PesedjetDatabaseContextFactory : IDesignTimeDbContextFactory<PesedjetDatabaseContext>
{
    private const string ConnectionStringName = "DefaultConnection";
    private const string MissingConnectionStringMessage =
        "ConnectionStrings:DefaultConnection not found. Set it with: " +
        "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\" --project Pesedjet.Server";

    public PesedjetDatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<PesedjetDatabaseContextFactory>()
            .Build();

        string connectionString = configuration.GetConnectionString(ConnectionStringName)
                                  ?? throw new InvalidOperationException(MissingConnectionStringMessage);

        var optionsBuilder = new DbContextOptionsBuilder<PesedjetDatabaseContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PesedjetDatabaseContext(optionsBuilder.Options);
    }
}
