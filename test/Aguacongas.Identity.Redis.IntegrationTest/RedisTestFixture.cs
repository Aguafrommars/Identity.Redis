// Project: aguacongas/Identity.Redis
// Copyright (c) 2025 @Olivier Lefebvre
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.IO;

namespace Aguacongas.Identity.Redis.IntegrationTest
{
    public class RedisTestFixture
    {
        public IConfigurationRoot Configuration { get; private set; }
        public IDatabase Database { get; private set; }
    
        public string TestDb { get; } = DateTime.Now.ToString("s");
        public RedisTestFixture()
        {
            var builder = new ConfigurationBuilder();
            Configuration = builder
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../../../../testsettings.json"))
                .Build();

            var options = new ConfigurationOptions();
            options.EndPoints.Add(Configuration.GetValue<string>("RedisOptions:HostAndPort"));
            options.AllowAdmin = true;

            var multiplexer = ConnectionMultiplexer.Connect(options);
            Database = multiplexer.GetDatabase();
            var server = multiplexer.GetServer(options.EndPoints[0]);
            server.FlushDatabase();
        }
    }
}
