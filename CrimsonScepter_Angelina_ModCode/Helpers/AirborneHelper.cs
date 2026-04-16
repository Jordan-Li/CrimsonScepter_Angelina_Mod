using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

public static class AirborneHelper
{
    public static bool IsAirborne(Creature? target)
    {
        return (target?.GetPower<FlyPower>()?.Amount ?? 0) > 0;
    }
}