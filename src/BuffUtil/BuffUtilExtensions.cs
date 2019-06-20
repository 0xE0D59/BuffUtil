using PoeHUD.Models;
using PoeHUD.Poe.Components;

namespace BuffUtil
{
    public static class BuffUtilExtensions
    {
        public static bool IsMonster(this EntityWrapper entity)
        {
            return entity != null && entity.HasComponent<Monster>();
        }

        public static bool IsDamageableMonster(this EntityWrapper entity)
        {
            return IsMonster(entity) && entity.IsValid && entity.IsAlive &&
                   entity.IsHostile &&
                   !entity.Invincible && !entity.CannotBeDamaged;
        }
    }
}