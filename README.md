![protoculture](protoculture.png)
# Postgres Embedded for .NET

![Build Status](https://github.com/atrauzzi/protoculture-dotnet-postgres/actions/workflows/publish.yml/badge.svg)
[![nuget](https://img.shields.io/nuget/v/Protoculture.Postgres.Embedded)](https://www.nuget.org/packages/Protoculture.Postgres.Embedded)
[![GitHub license](https://img.shields.io/github/license/atrauzzi/protoculture-dotnet-postgres)](https://github.com/atrauzzi/protoculture-dotnet-postgres/blob/main/LICENSE.txt)

## Features

  - Minimal and unobtrusive
  - Zero-ops for running a postgres server in .NET 
  - No dependency downloads at runtime, postgres binaries are bundled
  - Multi-platform and multi-architecture capable
  - Implements `IAsyncDisposable` and `IDisposable`
  - Focused on developer friendliness

## Installing

```bash
dotnet add package Protoculture.Postgres.Embedded
```

This package can be found both [on NuGet](https://www.nuget.org/packages/Protoculture.Postgres.Embedded) as well as over at its [package registry](https://github.com/atrauzzi/protoculture-dotnet-postgres/packages).

> note: If you're running on Windows, you will need to make sure the machine you're running on has [this VC redist](https://aka.ms/vs/17/release/vc_redist.x64.exe) installed (`vcruntime140.dll`).

## Usage

```c#
    await using var postgres = new EmbeddedPostgres();
    
    await postgres.Start();
    
    var socketConnectionString = postgres.Configuration.SocketConnectionString;
    var tcpConnectionString = postgres.Configuration.TcpConnectionString;
```

> note: The embedded postgres server runs as the same user as your application, this means that you cannot run your application as administrator as postgres will refuse to start.

### How do I use this with xUnit?

I'm currently working on coming up with an appropriate combination of libraries and documentation that will build on top of this project.

In the meantime however, if you're looking to get started with integration testing using this library in xUnit, consider the following:

```c#
public class EmbeddedPostgresFixture : IAsyncLifetime
{
    public readonly EmbeddedPostgres Server;

    public EmbeddedPostgresFixture()
    {
        Server = new(new()
        {
            Transient = true,
        });
    }

    public async Task InitializeAsync()
    {
        await Server.Start();
    }

    public async Task DisposeAsync()
    {
        await Server.Stop();
    }
}

[CollectionDefinition(nameof(EmbeddedPostgresFixture))]
public class EmbeddedPostgresCollectionFixture : ICollectionFixture<EmbeddedPostgresFixture>
{
}
```

With the above in place, you can inject `EmbeddedPostgres` into the constructor of your test classes using the following annotation:

```c#
[Collection(nameof(EmbeddedPostgresFixture))]
public class MyTests : IAsyncLifetime
{
    private readonly EmbeddedPostgres embeddedPostgres;

    public MyTests(EmbeddedPostgresFixture embeddedPostgresFixture)
    {
        embeddedPostgres = embeddedPostgresFixture.Server;
    }

    // ...
```

This will get you an instance of the server that is run only once and shared across all tests. Again, I hope to offer more in the near future to make testing with Entity Framework a snap, stay tuned!

## Meta

This project was started in January of 2022. 

### Purpose

The first use case for this project is as an easy zero-ops solution for integration testing in .NET projects. 

My hope is that it can grow into a 1st class option for any .NET developers looking to embed postgres in their applications.

### How does it work?
Nothing too magical, postgres binaries for each supported platform are distributed with this package. A thin layer of C# code provides a bridge for configuration, conventions and process lifecycle.

### Inspiration

 - A desire for a low ceremony, fully compatible option when writing integration tests
 - Early discussion around this project took place over at [this ticket](https://github.com/npgsql/npgsql/issues/4266) on the npgsql github issue tracker
 - [mysticmind/mysticmind-postgresembed](https://github.com/mysticmind/mysticmind-postgresembed/issues/10)
 - [aivascu/EntityFrameworkCore.AutoFixture](https://github.com/aivascu/EntityFrameworkCore.AutoFixture/issues/101)
 - SQLite

