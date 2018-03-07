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
            Configuration = builder.AddUserSecrets<UserStoreTest>()
                .AddEnvironmentVariables()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\testsettings.json"))
                .Build();

            var hostAndPor = Configuration.GetValue<string>("RedisOptions:HostAndPort");
            var multiplexer = ConnectionMultiplexer.Connect(hostAndPor);
            Database = multiplexer.GetDatabase();
            //var server = multiplexer.GetServer(hostAndPor);
            //server.FlushDatabase();
        }
    }
}
