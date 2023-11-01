// Project: aguacongas/Identity.Firebase
// Copyright (c) 2020 @Olivier Lefebvre
using Microsoft.AspNetCore.Identity;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.Identity.Redis.Test
{
    public class RoleStoreTest
    {
        [Fact]
        public async Task RoleStoreMethodsThrowWhenDisposedTest()
        {
            var dbMock = new Mock<IDatabase>();
            var store = new RoleStore<IdentityRole>(dbMock.Object);
            store.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.FindByIdAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.FindByNameAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.GetRoleIdAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.GetRoleNameAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.SetRoleNameAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.CreateAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.UpdateAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>  store.DeleteAsync(null));
        }

        [Fact]
        public async Task RoleStorePublicNullCheckTest()
        {
            Assert.Throws<ArgumentNullException>("db", () => new RoleStore<IdentityRole>(null));
            var dbMock = new Mock<IDatabase>();
            var store = new RoleStore<IdentityRole>(dbMock.Object);
            await Assert.ThrowsAsync<ArgumentNullException>("role", () =>  store.GetRoleIdAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", () =>  store.GetRoleNameAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", () =>  store.SetRoleNameAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", () =>  store.CreateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", () =>  store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", () =>  store.DeleteAsync(null));
        }

    }
}
