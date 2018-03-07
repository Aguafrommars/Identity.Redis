using Aguacongas.Redis;
using Aguacongas.Redis.TokenManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Aguacongas.Identity.Redis.IntegrationTest
{
    public class RedisTestFixture
    {
        public IConfigurationRoot Configuration { get; private set; }
        public RedisOptions RedisOptions { get; private set; }

        public string TestDb { get; } = DateTime.Now.ToString("s");
        public RedisTestFixture()
        {
            var builder = new ConfigurationBuilder();
            Configuration = builder.AddUserSecrets<UserStoreTest>()
                .AddEnvironmentVariables()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\testsettings.json"))
                .Build();

            RedisOptions = new RedisOptions();
            Configuration.GetSection("RedisOptions").Bind(RedisOptions);
        }
    }
}
