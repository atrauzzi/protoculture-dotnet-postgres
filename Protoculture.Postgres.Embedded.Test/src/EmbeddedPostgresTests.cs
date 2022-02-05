using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Npgsql;
using Protoculture.Postgres.Embedded.Extensions;
using Xunit;

namespace Protoculture.Postgres.Embedded.Test;

public class EmbeddedPostgresTests
{
    [Fact]
    public async Task ItStarts()
    {
        await using var server = new EmbeddedPostgres();

        await server.Start();

        server.Running.Should().BeTrue();
    }

    [Fact]
    public async Task ItStops()
    {
        var server = new EmbeddedPostgres();

        await server.Start();
        await server.Stop();

        server.Running.Should().BeFalse();
    }
    
    [Fact]
    public async Task ItCanBeReached()
    {
        await using var server = new EmbeddedPostgres();
        
        var connectionString = server.Configuration.BestConnectionString;

        await using var connection = new NpgsqlConnection(connectionString.ForDatabase("postgres").ConnectionString);
        await using var command = new NpgsqlCommand("select true", connection);

        await server.Start();
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        result.Should().BeOfType<bool>().Subject.Should().BeTrue();
    }

    [Fact]
    public async Task ItShouldCleanUpAfterItselfWhenTransient()
    {
        string basePath;
        
        await using (var server = new EmbeddedPostgres(new()
        {
            Transient = true,
        }))
        {
            await server.Start();

            basePath = server.Configuration.BasePath;
        }

        Directory.Exists(basePath).Should().BeFalse();
    }

    [Fact]
    public async Task ItShouldBeAbleToCreateDatabases()
    {
        await using var server = new EmbeddedPostgres(new()
        {
            Transient = true,
        });

        await server.Start();
        
        var connectionString = server.Configuration.BestConnectionString;

        await using var connection = new NpgsqlConnection(connectionString.ForDatabase("postgres").ConnectionString);
        await using var command = new NpgsqlCommand("select count(*) from pg_database where datname='semper_ubi_sub_ubi'", connection);

        await server.CreateDatabase("semper_ubi_sub_ubi");
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        result.Should().BeOfType<long>().Subject.Should().Be(1);
    }

    [Fact]
    public async Task ItCanCreateTables()
    {
        await using var server = new EmbeddedPostgres(new()
        {
            Transient = true,
        });

        await server.Start();
        
        var connectionString = server.Configuration.BestConnectionString;

        await using var connection = new NpgsqlConnection(connectionString.ForDatabase("postgres").ConnectionString);
        await using var createCommand = new NpgsqlCommand("create table to_be_flipped(id uuid)", connection);
        await using var checkCommand = new NpgsqlCommand("select count(*) from pg_catalog.pg_tables where tablename='to_be_flipped'", connection);

        await connection.OpenAsync();
        await createCommand.ExecuteNonQueryAsync();
        var result = await checkCommand.ExecuteScalarAsync();

        result.Should().BeOfType<long>().Subject.Should().Be(1);
    }
}
