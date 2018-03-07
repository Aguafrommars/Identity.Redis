using Aguacongas.Redis;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Aguacongas.Identity.Redis.IntegrationTest
{
    internal class UserOnlyStoreStub : UserOnlyStore<TestUser>
    {
        private readonly string _testDb;

        public UserOnlyStoreStub(string testDb, IRedisClient client, IdentityErrorDescriber describer = null) : base(client, describer)
        {
            _testDb = testDb;
        }

        protected override void SetIndex(Dictionary<string, object> rules, string key, object index)
        {
            var indexOverride = new Dictionary<string, object>();
            var jObject = rules.Where(kv => kv.Key == _testDb).Select(kv => kv.Value).FirstOrDefault() as JObject;
            rules.Clear();
            if (jObject != null)
            {
                foreach(var o in jObject)
                {
                    indexOverride.Add(o.Key, o.Value);
                }
            }
            
            indexOverride[key] = index;
            base.SetIndex(rules, _testDb, indexOverride);
        }

        protected override string GetRedisPath(params string[] objectPath)
        {
            return _testDb + "/" + base.GetRedisPath(objectPath);
        }
    }
}
