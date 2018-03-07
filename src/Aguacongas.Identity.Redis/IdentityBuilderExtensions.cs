// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Reflection;
using Aguacongas.Redis;
using Aguacongas.Redis.TokenManager;
using Aguacongas.Identity.Redis;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        /// <param name="url">Redis url</param>
        /// <param name="getTokenAccess"><see cref="ITokenAccess"/> factory function</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddRedisStores(this IdentityBuilder builder, string url, Func<IServiceProvider, ITokenAccess> getTokenAccess)
        {
            AddStores(builder.Services, builder.UserType, builder.RoleType);
            builder.Services.AddScoped<HttpClient>()
                .AddScoped<IRedisClient>(provider => new RedisClient(provider.GetRequiredService<HttpClient>(), provider.GetRequiredService<IRedisTokenManager>(), url, new JsonSerializerSettings()))
                .AddScoped<IRedisTokenManager, AuthTokenManager>()
                .AddSingleton(getTokenAccess);

            return builder;
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="url">Redis url</param>
        /// <param name="getTokenManager"><see cref="IRedisTokenManager"/> factory function</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddRedisStores(this IdentityBuilder builder, string url, Func<IServiceProvider, IRedisTokenManager> getTokenManager)
        {
            AddStores(builder.Services, builder.UserType, builder.RoleType);
            builder.Services.AddScoped<HttpClient>()
                .AddScoped<IRedisClient>(provider => new RedisClient(provider.GetRequiredService<HttpClient>(), provider.GetRequiredService<IRedisTokenManager>(), url, new JsonSerializerSettings()))
                .AddSingleton(provider => getTokenManager(provider));

            return builder;
        }

        /// <summary>
        /// Adds an Redis implementation of identity stores.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="url">Redis url</param>
        /// <param name="configure">Action to configure AuthTokenOptions</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddRedisStores(this IdentityBuilder builder, string url, Action<AuthTokenOptions> configure)
        {
            builder.Services.Configure(configure);

            return builder
                .AddRedisStores(url, provider =>
                {
                    var options = provider.GetRequiredService<IOptions<AuthTokenOptions>>();
                    var json = JsonConvert.SerializeObject(options?.Value ?? throw new ArgumentNullException(nameof(options)));
                    return GoogleCredential.FromJson(json)
                        .CreateScoped("https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/Redis.database")
                        .UnderlyingCredential;
                });
        }

        private static void AddStores(IServiceCollection services, Type userType, Type roleType)
        {
            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<string>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a user that derives from IdentityUser<string>.");
            }

            var userOnlyStoreType = typeof(UserOnlyStore<>).MakeGenericType(userType);

            if (roleType != null)
            {
                var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<string>));
                if (identityRoleType == null)
                {
                    throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a role that derives from IdentityRole<string>.");
                }

                var userStoreType = typeof(UserStore<,>).MakeGenericType(userType, roleType);
                var roleStoreType = typeof(RoleStore<>).MakeGenericType(roleType);

                services.TryAddScoped(typeof(UserOnlyStore<>).MakeGenericType(userType), userOnlyStoreType);
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            }
            else
            {   // No Roles
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userOnlyStoreType);
            }
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType;
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                var genericType = type.IsGenericType ? type: null;
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