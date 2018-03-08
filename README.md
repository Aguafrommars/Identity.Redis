# Identity.Redis
[ASP.NET Identity](https://github.com/aspnet/identity) Redis Provider

[![Build status](https://ci.appveyor.com/api/projects/status/tmh3ib2s64ay2sc7?svg=true)](https://ci.appveyor.com/project/aguacongas/identity-redis)
[![Latest Code coveragre](https://aguacongas.github.io/Identity.Redis/latest/badge_linecoverage.svg)](https://aguacongas.github.io/Identity.Redis/latest)

Nuget packages
--------------
|Aguacongas.Redis|Aguacongas.Identity.Redis|
|:------:|:------:|
[![][Aguacongas.Redis-badge]][Aguacongas.Redis-nuget]|[![][Aguacongas.Identity.Redis-badge]][Aguacongas.Identity.Redis-nuget]|
[![][Aguacongas.Redis-downloadbadge]][Aguacongas.Redis-nuget]|[![][Aguacongas.Identity.Redis-downloadbadge]][Aguacongas.Identity.Redis-nuget]|


[Aguacongas.Redis-badge]: https://img.shields.io/nuget/v/Aguacongas.Redis.svg
[Aguacongas.Redis-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.Redis.svg
[Aguacongas.Redis-nuget]: https://www.nuget.org/packages/Aguacongas.Redis/

[Aguacongas.Identity.Redis-badge]: https://img.shields.io/nuget/v/Aguacongas.Identity.Redis.svg
[Aguacongas.Identity.Redis-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.Identity.Redis.svg
[Aguacongas.Identity.Redis-nuget]: https://www.nuget.org/packages/Aguacongas.Identity.Redis/



## Setup

You setup Redis stores using one `AddRedisStores` extension method

You can setup Redis stores with a redis configuration string:

    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRedisStores("localhost")
        .AddDefaultTokenProviders();

Or with an Action receiving an instance of a ConfigurationOptions:


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

Or, you ca use `AddRedisStores`  with a `Func<IdentityProvider, IDatabase>`:

    var multiplexer = ConnectionMultiplexer.Connect("localhost");

    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRedisStores(provider => multiplexer.GetDatabase())
        .AddDefaultTokenProviders();



## Sample

The sample is a copy of [IdentitySample.Mvc](https://github.com/aspnet/Identity/tree/dev/samples/IdentitySample.Mvc) sample using a Redis database.  

