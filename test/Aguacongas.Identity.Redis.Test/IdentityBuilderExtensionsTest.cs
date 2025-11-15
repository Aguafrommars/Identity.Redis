// Project: aguacongas/Identity.Redis
// Copyright (c) 2025 @Olivier Lefebvre
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Aguacongas.Identity.Redis.Test
{
    public class IdentityBuilderExtensionsTest
    {
        [Fact]
        public void AddRedisStores_with_ConfigurationStringTest()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            var builder = new ConfigurationBuilder();
            var configuration = builder
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../../../../testsettings.json"))
                .Build();

            var services = new ServiceCollection();
            services.AddLogging()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddRedisStores("localhost");

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IUserStore<IdentityUser>>();
            provider.GetRequiredService<IRoleStore<IdentityRole>>();
        }

        [Fact]
        public void AddRedisStores_with_ConfigurationOptionsTest()
        {
            var services = new ServiceCollection();
            services.AddLogging()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddRedisStores(options =>
                {
                    options.EndPoints.Add("localhost");
                });

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IUserStore<IdentityUser>>();
            provider.GetRequiredService<IRoleStore<IdentityRole>>();
        }

        [Fact]
        public void AddRedistore_without_roleTest()
        {
            var services = new ServiceCollection();
            var builder = new IdentityBuilder(typeof(IdentityUser), services);

            builder.AddRedisStores("localhost");

            var provider = services.AddLogging()
                .BuildServiceProvider();

            provider.GetRequiredService<IUserStore<IdentityUser>>();
        }
    }
}
