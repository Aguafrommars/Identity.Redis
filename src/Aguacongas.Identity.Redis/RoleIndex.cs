using Aguacongas.Redis;

namespace Aguacongas.Identity.Redis
{
    public class RoleIndex : RedisIndex
    {
        public RoleIndex()
        {
            On = "NormalizedName";
        }
    }
}