using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;
using Xunit;

namespace Aguacongas.Identity.Redis.Test
{
    public class IdentityBuilderExtensionsTest
    {
        [Fact]
        public void AddRedisStores_with_ConfigurationStringTest()
        {
            var builder = new ConfigurationBuilder();
            var configuration = builder.AddUserSecrets<IdentityBuilderExtensionsTest>()
                .AddEnvironmentVariables()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\testsettings.json"))
                .Build();

            var services = new ServiceCollection();
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRedisStores("localhost");

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IUserStore<IdentityUser>>();
            provider.GetRequiredService<IRoleStore<IdentityRole>>();
        }

        [Fact]
        public void AddRedisStores_with_ConfigurationOptionsTest()
        {
            var services = new ServiceCollection();
            services.AddIdentity<IdentityUser, IdentityRole>()
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

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IUserStore<IdentityUser>>();
        }
    }
}
