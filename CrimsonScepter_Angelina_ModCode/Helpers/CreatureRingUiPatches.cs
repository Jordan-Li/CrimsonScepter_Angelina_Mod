using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.SetCreature))]
internal static class CreatureRingUiAttachPatch
{
    private static void Postfix(NHealthBar __instance, Creature creature)
    {
        CreatureRingUi.Attach(__instance, creature);
    }
}

[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.RefreshValues))]
internal static class CreatureRingUiRefreshPatch
{
    private static void Postfix(NHealthBar __instance)
    {
        CreatureRingUi.Refresh(__instance);
    }
}

[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.UpdateLayoutForCreatureBounds))]
internal static class CreatureRingUiLayoutPatch
{
    private static void Postfix(NHealthBar __instance)
    {
        CreatureRingUi.RefreshLayout(__instance);
    }
}

[HarmonyPatch(typeof(NCreature), "OnPowerApplied")]
internal static class CreatureRingUiPowerAppliedPatch
{
    private static void Postfix(NCreature __instance, PowerModel power)
    {
        CreatureRingUi.Refresh(power.Owner);
    }
}

[HarmonyPatch(typeof(NCreature), "OnPowerRemoved")]
internal static class CreatureRingUiPowerRemovedPatch
{
    private static void Postfix(NCreature __instance)
    {
        CreatureRingUi.Refresh(__instance.Entity);
    }
}

[HarmonyPatch(typeof(NCreature), "OnPowerIncreased")]
internal static class CreatureRingUiPowerAmountChangedPatch
{
    private static void Postfix(NCreature __instance)
    {
        CreatureRingUi.Refresh(__instance.Entity);
    }
}
