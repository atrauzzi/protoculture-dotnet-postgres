using Protoculture.Postgres.Embedded;

var server = new EmbeddedPostgres(new()
{
    ShowOutput = true,
    Port = 31337,
    BasePath = "/tmp/protoculture-postgres-embedded", 
});

await server.Start();
Console.WriteLine("Is ready!");
