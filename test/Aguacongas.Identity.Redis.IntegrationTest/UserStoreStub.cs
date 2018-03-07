using Aguacongas.Redis;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.Identity.Redis.IntegrationTest
{
    internal class UserStoreStub: UserStore<TestUser, TestRole>
    {
        private readonly string _testDb;

        public UserStoreStub(string testDb, IRedisClient client, UserOnlyStore<TestUser> userOnlyStore, IdentityErrorDescriber describer = null) : base(client, userOnlyStore, describer)
        {
            _testDb = testDb;
        }

        protected override string GetRedisPath(params string[] objectPath)
        {
            return _testDb + "/" + base.GetRedisPath(objectPath);
        }
    }
}
