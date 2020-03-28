// Project: aguacongas/Identity.Firebase
// Copyright (c) 2020 @Olivier Lefebvre
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.Identity.Redis
{
    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    public class RoleStore<TRole> : RoleStore<TRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>>
        where TRole : IdentityRole<string>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="RoleStore{TRole}"/>.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(IDatabase db, IdentityErrorDescriber describer = null) : base(db, describer) { }
    }

    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    public class RoleStore<TRole, TKey> : RoleStore<TRole, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="RoleStore{TRole}"/>.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(IDatabase db, IdentityErrorDescriber describer = null) : base(db, describer) { }
    }


    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    /// <typeparam name="TUserRole">The type of the class representing a user role.</typeparam>
    /// <typeparam name="TRoleClaim">The type of the class representing a role claim.</typeparam>
    [SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "Identity store implementation")]
    [SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Nothing to dispose")]
    [SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Identity store implementation")]
    [SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Nothing to dispose")]
    [SuppressMessage("Critical Code Smell", "S1006:Method overrides should not change parameter defaults", Justification = "<Pending>")]
    public class RoleStore<TRole, TKey, TUserRole, TRoleClaim> :
        IQueryableRoleStore<TRole>,
        IRoleClaimStore<TRole>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
    {
        private const string RolesRedisKey = "{roles}";
        private const string RolesConcurencyStampIndexKey = "{roles}-concurency";
        private const string RolesNameIndexKey = "{role}-names";
        private const string RoleClaimsRedisKey = "{role}-claims";
        private const string RoleClaimsKeyPrefix = "{role}-claims-";

        private readonly IDatabase _db;
        private bool _disposed;

        /// <summary>
        /// A navigation property for the roles the store contains.
        /// </summary>
        public IQueryable<TRole> Roles
        {
            get
            {
                var results = _db.HashGetAll(RolesRedisKey);
                return results.Select(u => JsonConvert.DeserializeObject<TRole>(u.Value))
                    .AsQueryable();
            }
        }


        /// <summary>
        /// Constructs a new instance of <see cref="RoleStore{TRole, TUserRole, TRoleClaim}"/>.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(IDatabase db, IdentityErrorDescriber describer = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
        }

        /// <summary>
        /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
        /// </summary>
        public IdentityErrorDescriber ErrorDescriber { get; set; }

        /// <summary>
        /// Creates a new role in a store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to create in the store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
        public async virtual Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            var roleId = ConvertIdToString(role.Id);
            role.ConcurrencyStamp = "0";

            var tran = _db.CreateTransaction();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable S1854 // Dead stores should be removed
            var roleNotExistsCondition = tran.AddCondition(Condition.HashNotExists(RolesRedisKey, roleId));
#pragma warning restore S1854 // Dead stores should be removed
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashSetAsync(RolesRedisKey, roleId, JsonConvert.SerializeObject(role));
            tran.HashSetAsync(RolesConcurencyStampIndexKey, roleId, GetConcurrencyStamp(role));
            tran.HashSetAsync(RolesNameIndexKey, role.NormalizedName, roleId);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            if (!await tran.ExecuteAsync().ConfigureAwait(false))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = nameof(roleNotExistsCondition),
                    Description = $"User id: {role.Id} already exists"
                });
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Updates a role in a store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to update in the store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
        public async virtual Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));


            var roleId = ConvertIdToString(role.Id);

            var tran = _db.CreateTransaction();
            tran.AddCondition(Condition.HashEqual(RolesConcurencyStampIndexKey, roleId, GetConcurrencyStamp(role)));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashSetAsync(RolesRedisKey, roleId, JsonConvert.SerializeObject(role));
            var concurency = tran.HashIncrementAsync(RolesConcurencyStampIndexKey, roleId);
            tran.HashSetAsync(RolesNameIndexKey, role.NormalizedName, roleId);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            if (!await tran.ExecuteAsync().ConfigureAwait(false))
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            role.ConcurrencyStamp = concurency.Result.ToString();

            return IdentityResult.Success;
        }

        /// <summary>
        /// Deletes a role from the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to delete from the store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
        public async virtual Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            var roleId = ConvertIdToString(role.Id);

            var tran = _db.CreateTransaction();
            tran.AddCondition(Condition.HashEqual(RolesConcurencyStampIndexKey, roleId, GetConcurrencyStamp(role)));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashDeleteAsync(RolesRedisKey, roleId);
            tran.HashDeleteAsync(RolesNameIndexKey, role.NormalizedName);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            if (!await tran.ExecuteAsync().ConfigureAwait(false))
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Gets the ID for a role from the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose ID should be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the ID of the role.</returns>
        public virtual Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            return Task.FromResult(ConvertIdToString(role.Id));
        }

        /// <summary>
        /// Gets the name of a role from the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose name should be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
        public virtual Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            return Task.FromResult(role.Name);
        }

        /// <summary>
        /// Sets the name of a role in the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose name should be set.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            role.Name = roleName;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Finds the role who has the specified ID as an asynchronous operation.
        /// </summary>
        /// <param name="roleId">The role ID to look for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
        public virtual async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var response = await _db.HashGetAsync(RolesRedisKey, roleId).ConfigureAwait(false);
            if (response.HasValue)
            {
                var role = JsonConvert.DeserializeObject<TRole>(response);
                role.ConcurrencyStamp = await _db.HashGetAsync(RolesConcurencyStampIndexKey, roleId)
                    .ConfigureAwait(false);

                return role;
            }
            return default;
        }

        /// <summary>
        /// Finds the role who has the specified normalized name as an asynchronous operation.
        /// </summary>
        /// <param name="normalizedRoleName">The normalized role name to look for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
        public virtual async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var roleId = await _db.HashGetAsync(RolesNameIndexKey, normalizedRoleName).ConfigureAwait(false);
            if (!roleId.HasValue)
            {
                return default;
            }

            return await FindByIdAsync(roleId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a role's normalized name as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose normalized name should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
        public virtual Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        /// <summary>
        /// Set a role's normalized name as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose normalized name should be set.</param>
        /// <param name="normalizedName">The normalized name to set</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Dispose the stores
        /// </summary>
        public void Dispose() => _disposed = true;

        /// <summary>
        /// Get the claims associated with the specified <paramref name="role"/> as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose claims should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a role.</returns>
        public async virtual Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));

            var roleId = ConvertIdToString(role.Id);

            var response = await _db.HashGetAsync(RoleClaimsRedisKey, roleId).ConfigureAwait(false);
            if (response.HasValue)
            {
                var claims = JsonConvert.DeserializeObject<List<TRoleClaim>>(response);
                return claims.Select(c => c.ToClaim()).ToList();
            }

            return new List<Claim>(0);
        }

        /// <summary>
        /// Adds the <paramref name="claim"/> given to the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The role to add the claim to.</param>
        /// <param name="claim">The claim to add to the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));
            AssertNotNull(claim, nameof(claim));

            var roleClaims = await GetRoleClaimsAsync(role).ConfigureAwait(false);

            roleClaims.Add(CreateRoleClaim(role, claim));

            var roleId = ConvertIdToString(role.Id);

            await Task.WhenAll(_db.HashSetAsync(RoleClaimsRedisKey, roleId, JsonConvert.SerializeObject(roleClaims)),
                _db.HashSetAsync(RoleClaimsKeyPrefix + claim.Type, roleId, claim.Value))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Removes the <paramref name="claim"/> given from the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The role to remove the claim from.</param>
        /// <param name="claim">The claim to remove from the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async virtual Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            AssertNotNull(role, nameof(role));
            AssertNotNull(claim, nameof(claim));

            var roleClaims = await GetRoleClaimsAsync(role).ConfigureAwait(false);

            roleClaims.RemoveAll(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);

            var roleId = ConvertIdToString(role.Id);

            await Task.WhenAll(_db.HashSetAsync(RoleClaimsRedisKey, roleId, JsonConvert.SerializeObject(roleClaims)),
                _db.HashDeleteAsync(RoleClaimsKeyPrefix + claim.Type, roleId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Creates an entity representing a role claim.
        /// </summary>
        /// <param name="role">The associated role.</param>
        /// <param name="claim">The associated claim.</param>
        /// <returns>The role claim entity.</returns>
        protected virtual TRoleClaim CreateRoleClaim(TRole role, Claim claim)
            => new TRoleClaim { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };

        /// <summary>
        /// Converts the provided <paramref name="id"/> to a strongly typed key object.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>An instance of <typeparamref name="TKey"/> representing the provided <paramref name="id"/>.</returns>
        public virtual TKey ConvertIdFromString(string id)
        {
            if (id == null)
            {
                return default;
            }
            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }

        /// <summary>
        /// Converts the provided <paramref name="id"/> to its string representation.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>An <see cref="string"/> representation of the provided <paramref name="id"/>.</returns>
        public virtual string ConvertIdToString(TKey id)
        {
            if (Equals(id, default(TKey)))
            {
                return null;
            }
            return id.ToString();
        }

        protected virtual async Task<List<TRoleClaim>> GetRoleClaimsAsync(TRole role)
        {
            var roleId = ConvertIdToString(role.Id);

            var response = await _db.HashGetAsync(RoleClaimsRedisKey, roleId).ConfigureAwait(false);
            if (response.HasValue)
            {
                return JsonConvert.DeserializeObject<List<TRoleClaim>>(response);
            }

            return new List<TRoleClaim>();
        }

        private static int? GetConcurrencyStamp(TRole role)
        {
            if (int.TryParse(role.ConcurrencyStamp, out int stamp))
            {
                return stamp;
            }
            return null;
        }

        private static void AssertNotNull(object p, string pName)
        {
            if (p == null)
            {
                throw new ArgumentNullException(pName);
            }
        }
    }
}
