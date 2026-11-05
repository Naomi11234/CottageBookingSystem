
using System;
using CottageBookingSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CottageBookingSystem.Data.PostgreSql;

public class PostgresDbContextFactory
    : IDesignTimeDbContextFactory<PostgresDbContext>
{
    public PostgresDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<PostgresDbContextFactory>()
            .Build();

        var connectionString =
            configuration.GetConnectionString("PostgresConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgresConnection was not found in User Secrets.");
        }

        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(
                    "__EFMigrationsHistory_PostgreSql"))
            .Options;

        return new PostgresDbContext(options);
    }
}
