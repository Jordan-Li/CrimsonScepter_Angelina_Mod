using BaseLib.Utils;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

internal static class SpellEnchantmentCompatibilityPatches
{
    private static readonly AccessTools.FieldRef<Momentum, int> MomentumExtraDamageRef =
        AccessTools.FieldRefAccess<Momentum, int>("_extraDamage");

    private static bool ShouldApplyToSpellCard(EnchantmentModel enchantment, ValueProp props)
    {
        if (!props.HasFlag(ValueProp.Move) || !props.HasFlag(ValueProp.Unpowered) || !enchantment.HasCard)
        {
            return false;
        }

        return enchantment.Card is AngelinaCard { IsSpell: true };
    }

    private static bool ShouldApplyToSpellCard(CardModel? cardSource, ValueProp props)
    {
        if (!props.HasFlag(ValueProp.Move) || !props.HasFlag(ValueProp.Unpowered))
        {
            return false;
        }

        return cardSource is AngelinaCard { IsSpell: true };
    }

    [HarmonyPatch(typeof(Sharp), nameof(Sharp.EnchantDamageAdditive))]
    private static class SharpSpellPatch
    {
        private static void Postfix(Sharp __instance, ref decimal __result, decimal originalDamage, ValueProp props)
        {
            if (__result != 0m || !ShouldApplyToSpellCard(__instance, props))
            {
                return;
            }

            __result = __instance.Amount;
        }
    }

    [HarmonyPatch(typeof(Momentum), nameof(Momentum.EnchantDamageAdditive))]
    private static class MomentumSpellPatch
    {
        private static void Postfix(Momentum __instance, ref decimal __result, decimal originalDamage, ValueProp props)
        {
            if (__result != 0m || !ShouldApplyToSpellCard(__instance, props))
            {
                return;
            }

            __result = MomentumExtraDamageRef(__instance);
        }
    }

    [HarmonyPatch(typeof(Instinct), nameof(Instinct.EnchantDamageMultiplicative))]
    private static class InstinctSpellPatch
    {
        private static void Postfix(Instinct __instance, ref decimal __result, decimal originalDamage, ValueProp props)
        {
            if (__result != 1m || !ShouldApplyToSpellCard(__instance, props))
            {
                return;
            }

            __result = 2m;
        }
    }

    [HarmonyPatch(typeof(Corrupted), nameof(Corrupted.EnchantDamageMultiplicative))]
    private static class CorruptedSpellPatch
    {
        private static void Postfix(Corrupted __instance, ref decimal __result, decimal originalDamage, ValueProp props)
        {
            if (__result != 1m || !ShouldApplyToSpellCard(__instance, props))
            {
                return;
            }

            __result = 1.5m;
        }
    }

    [HarmonyPatch(typeof(Vigorous), nameof(Vigorous.EnchantDamageAdditive))]
    private static class VigorousSpellPatch
    {
        private static void Postfix(Vigorous __instance, ref decimal __result, decimal originalDamage, ValueProp props)
        {
            if (__result != 0m || !ShouldApplyToSpellCard(__instance, props))
            {
                return;
            }

            if (__instance.Status != MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Normal)
            {
                return;
            }

            __result = __instance.Amount;
        }
    }

    [HarmonyPatch(typeof(MiniatureCannon), nameof(MiniatureCannon.ModifyDamageAdditive))]
    private static class MiniatureCannonSpellPatch
    {
        private static void Postfix(MiniatureCannon __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (__result != 0m || !ShouldApplyToSpellCard(cardSource, props) || cardSource == null)
            {
                return;
            }

            if (!cardSource.IsUpgraded)
            {
                return;
            }

            if (dealer != __instance.Owner.Creature && cardSource.Owner != __instance.Owner)
            {
                return;
            }

            __result = __instance.DynamicVars["ExtraDamage"].BaseValue;
        }
    }

    [HarmonyPatch(typeof(MysticLighter), nameof(MysticLighter.ModifyDamageAdditive))]
    private static class MysticLighterSpellPatch
    {
        private static void Postfix(MysticLighter __instance, ref decimal __result, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (__result != 0m || !ShouldApplyToSpellCard(cardSource, props) || cardSource?.Enchantment == null)
            {
                return;
            }

            if (cardSource.Owner != __instance.Owner)
            {
                return;
            }

            __result = __instance.DynamicVars.Damage.IntValue;
        }
    }
}
