using Protoculture.Postgres.Embedded;

using var server = new EmbeddedPostgres(new()
{
    ShowOutput = true,
});

await server.Start();
Console.WriteLine("Is ready!");
