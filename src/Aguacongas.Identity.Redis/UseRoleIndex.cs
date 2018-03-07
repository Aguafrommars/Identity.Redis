using Aguacongas.Redis;

namespace Aguacongas.Identity.Redis
{
    public class UseRoleIndex: RedisIndexes
    {
        public UseRoleIndex()
        {
            On = new string[] { "UserId", "RoleId" };
        }
    }
}