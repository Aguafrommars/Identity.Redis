using Aguacongas.Redis;

namespace Aguacongas.Identity.Redis
{
    public class RoleClaimIndex : RedisIndex
    {
        public RoleClaimIndex()
        {
            On = "RoleId";
        }
    }
}