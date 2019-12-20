# Identity.Redis
[ASP.NET Identity](https://github.com/aspnet/AspNetCore/tree/master/src/Identity) Redis Provider

[![Build status](https://ci.appveyor.com/api/projects/status/tmh3ib2s64ay2sc7?svg=true)](https://ci.appveyor.com/project/aguacongas/identity-redis)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aguacongas_Identity.Redis&metric=alert_status)](https://sonarcloud.io/dashboard?id=aguacongas_Identity.Redis)

Nuget packages
--------------
|Aguacongas.Identity.Redis|
|:------:|
|[![][Aguacongas.Identity.Redis-badge]][Aguacongas.Identity.Redis-nuget]|
|[![][Aguacongas.Identity.Redis-downloadbadge]][Aguacongas.Identity.Redis-nuget]|

[Aguacongas.Identity.Redis-badge]: https://img.shields.io/nuget/v/Aguacongas.Identity.Redis.svg
[Aguacongas.Identity.Redis-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.Identity.Redis.svg
[Aguacongas.Identity.Redis-nuget]: https://www.nuget.org/packages/Aguacongas.Identity.Redis/

## Setup

You setup Redis stores using one `AddRedisStores` extension method

You can setup Redis stores with a redis configuration string:

    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRedisStores("localhost")
        .AddDefaultTokenProviders();

Or with an `Action` receiving an instance of a `StackExchange.Redis.ConfigurationOptions`:


    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRedisStores(options =>
        {
            options.EndPoints.Add("localhost:6379");
        })
        .AddDefaultTokenProviders();

Both methods can take a `int? database` parameter to specify the Redis database to use:

    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRedisStores(Configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), 1)
        .AddDefaultTokenProviders();

Or, you can use `AddRedisStores` with a `Func<IdentityProvider, StackExchange.Redis.IDatabase>`:

    var multiplexer = ConnectionMultiplexer.Connect("localhost");

    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRedisStores(provider => multiplexer.GetDatabase())
        .AddDefaultTokenProviders();

### Logging

A logger is automaticaly injected to the underlying `StackExchange.Redis.ConnectionMultiplexer` by `AddRedisStores` methods.  
This logger write traces, (LogLevel =  `LogLevel.Trace`), to enable it add a filter to your logging configuration:

    services.AddLogging(builder =>
    {
        builder.AddDebug()
            .AddConsole()
            .AddFilter("Aguacongas.Identity.Redis", LogLevel.Trace);
    })

> Obviously, if you use `AddRedisStores` with a `Func<IdentityProvider, StackExchange.Redis.IDatabase>` the logger is not injected automaticaly.

## Sample

The sample is a copy of [IdentitySample.Mvc](https://github.com/aspnet/Identity/tree/dev/samples/IdentitySample.Mvc) sample using a Redis database.  

## Tests

This library is tested using [Microsoft.AspNetCore.Identity.Specification.Tests](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.Specification.Tests/), the shared test suite for Asp.Net Identity Core store implementations.  
