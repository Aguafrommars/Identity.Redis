// Project: aguacongas/Identity.Redis
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.Identity.Redis;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Contains extension methods to <see cref="IdentityBuilder"/> for adding entity framework stores.
    /// </summary>
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="getDatabase"><see cref="IDatabase"/> factory function returning the redis database to use</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddRedisStores(this IdentityBuilder builder, Func<IServiceProvider, IDatabase> getDatabase)
        {
            AddStores(builder.Services, builder.UserType, builder.RoleType, getDatabase);

            return builder;
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="configure">Action to configure <see cref="ConfigurationOptions"/></param>
        /// <param name="database">(Optional) The redis database to use</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddRedisStores(this IdentityBuilder builder, Action<ConfigurationOptions> configure, int? database = null)
        {
            var services = builder.Services;

            services.Configure(configure)
                .AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
                    var redisLogger = CreateLogger(provider);
                    return ConnectionMultiplexer.Connect(options, redisLogger);
                });

            return builder.AddRedisStores(provider =>
                {
                    var options = provider.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
                    var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
                    return multiplexer.GetDatabase(database ?? (options.DefaultDatabase ?? -1));
                });
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="configuration">The redis configuration string</param>
        /// <param name="database">(Optional) The redis database to use</param>
        /// <param name="log">(Optional) a <see cref="TextWriter"/> to write log</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddRedisStores(this IdentityBuilder builder, string configuration, int? database = null)
        {
            var services = builder.Services;

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var redisLogger = CreateLogger(provider);

                return ConnectionMultiplexer.Connect(configuration, redisLogger);
            });

            return builder
                .AddRedisStores(provider =>
                {
                    var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
                    return multiplexer.GetDatabase(database ?? -1);
                });
        }

        private static RedisLogger CreateLogger(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger<RedisLogger>>();
            var redisLogger = logger != null ? new RedisLogger(logger) : null;
            return redisLogger;
        }

        private static void AddStores(IServiceCollection services, Type userType, Type roleType, Func<IServiceProvider, IDatabase> getDatabase)
        {
            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a user that derives from IdentityUser<TKey>.");
            }

            var keyType = identityUserType.GenericTypeArguments[0];
        
            var userOnlyStoreType = typeof(UserOnlyStore<,>).MakeGenericType(userType, keyType);

            if (roleType != null)   
            {
                var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
                if (identityRoleType == null)
                {
                    throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a role that derives from IdentityRole<TKey>.");
                }

                var userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, keyType);
                var roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, keyType);

                services.TryAddScoped(typeof(UserOnlyStore<,>).MakeGenericType(userType, keyType), provider => CreateStoreInstance(userOnlyStoreType, getDatabase(provider), provider.GetService<IdentityErrorDescriber>()));
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), provider => userStoreType.GetConstructor(new Type[] { typeof(IDatabase), userOnlyStoreType, typeof(IdentityErrorDescriber) })
                    .Invoke(new object[] { getDatabase(provider), provider.GetService(userOnlyStoreType), provider.GetService<IdentityErrorDescriber>() }));
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), provider => CreateStoreInstance(roleStoreType, getDatabase(provider), provider.GetService<IdentityErrorDescriber>()));
            }
            else
            {   // No Roles
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), provider => CreateStoreInstance(userOnlyStoreType, getDatabase(provider), provider.GetService<IdentityErrorDescriber>()));
            }
        }

        private static object CreateStoreInstance(Type storeType, IDatabase db, IdentityErrorDescriber errorDescriber)
        {
            var constructor = storeType.GetConstructor(new Type[] { typeof(IDatabase), typeof(IdentityErrorDescriber)});
            return constructor.Invoke(new object[] { db, errorDescriber });
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType;
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    return typeInfo;
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}