using Aguacongas.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.Identity.Redis
{
    public class LoginIndex : RedisIndexes
    {
        public LoginIndex()
        {
            On = new string[] { "ProviderKey", "UserId" };
        }
    }
}
