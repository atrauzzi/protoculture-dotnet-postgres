![protoculture](protoculture.png)
# Postgres Embedded for .NET

![Build Status](https://github.com/atrauzzi/protoculture-dotnet-postgres/actions/workflows/cicd.yml/badge.svg)

## Features

  - Minimal and unobtrusive
  - Zero-ops for running a postgres server in .NET 
  - Postgres binaries are bundled as part of the package
  - Multi-platform and multi-architecture
  - Implements `IAsyncDisposable` and `IDisposable`
  - Focused on developer friendliness
  
## Usage

```c#
    await using var postgres = new EmbeddedPostgres();
    
    await postgres.Start();
    
    var socketConnectionString = postgres.Configuration.SocketConnectionString;
    var tcpConnectionString = postgres.Configuration.TcpConnectionString;
```

## Meta

### Inspiration

 - SQLite
 - Early discussion around this project took place over at [this ticket](https://github.com/npgsql/npgsql/issues/4266) on the npgsql github issue tracker
 - [mysticmind/mysticmind-postgresembed](https://github.com/mysticmind/mysticmind-postgresembed/issues/10)
 - [aivascu/EntityFrameworkCore.AutoFixture](https://github.com/aivascu/EntityFrameworkCore.AutoFixture/issues/101)
 
