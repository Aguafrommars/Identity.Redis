using Aguacongas.Redis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.Identity.Redis
{
    public class UserIndex: RedisIndexes
    {
        public UserIndex()
        {
            On = new string[] { "NormalizedEmail", "NormalizedUserName" };
        }
    }
}
