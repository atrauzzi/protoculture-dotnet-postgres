![protoculture](protoculture.png)
# Postgres Embedded for .NET

![Build Status](https://github.com/atrauzzi/protoculture-dotnet-postgres/actions/workflows/cicd.yml/badge.svg)

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

This package can be found both on NuGet as well as over at its [package registry](https://github.com/atrauzzi/protoculture-dotnet-postgres/packages).

## Usage

```c#
    await using var postgres = new EmbeddedPostgres();
    
    await postgres.Start();
    
    var socketConnectionString = postgres.Configuration.SocketConnectionString;
    var tcpConnectionString = postgres.Configuration.TcpConnectionString;
```

## Meta

This project was started in January of 2022. My hope is that it can evolve into a 1st class option for all .NET developers who want to embed postgres in their applications.

### How does it work?

Nothing too magical, postgres binaries for each supported platform are statically distributed with this package. A thin layer of C# code provides a bridge for configuration, conventions and process lifecycle.

### Inspiration

 - A desire for a low ceremony, fully compatible option when writing integration tests
 - Early discussion around this project took place over at [this ticket](https://github.com/npgsql/npgsql/issues/4266) on the npgsql github issue tracker
 - [mysticmind/mysticmind-postgresembed](https://github.com/mysticmind/mysticmind-postgresembed/issues/10)
 - [aivascu/EntityFrameworkCore.AutoFixture](https://github.com/aivascu/EntityFrameworkCore.AutoFixture/issues/101)
 - SQLite

