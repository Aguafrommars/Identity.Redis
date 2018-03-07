using Aguacongas.Redis;

namespace Aguacongas.Identity.Redis
{
    public class UserClaimIndex : RedisIndexes
    {
        public UserClaimIndex()
        {
            On = new string[] { "UserId", "ClaimType" };
        }
    }
}
