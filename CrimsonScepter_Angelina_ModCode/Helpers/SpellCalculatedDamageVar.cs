using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

/// <summary>
/// Keeps spell-style calculated damage previews aligned with actual resolution.
/// </summary>
public sealed class SpellCalculatedDamageVar(ValueProp props) : CalculatedDamageVar(props)
{
    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        EnchantmentModel? enchantment = card.Enchantment;
        if (enchantment != null)
        {
            decimal enchantedBaseValue = card.DynamicVars.CalculationBase.BaseValue;
            enchantedBaseValue += enchantment.EnchantDamageAdditive(enchantedBaseValue, Props);
            enchantedBaseValue *= enchantment.EnchantDamageMultiplicative(enchantedBaseValue, Props);
            enchantedBaseValue = Math.Max(enchantedBaseValue, 0m);
            if (card.IsEnchantmentPreview)
            {
                PreviewValue = enchantedBaseValue;
            }
            else
            {
                EnchantedValue = enchantedBaseValue;
            }
        }

        decimal previewDamage = SpellHelper.ModifySpellValue(card.Owner.Creature, Calculate(target));
        if (runGlobalHooks)
        {
            ICombatState? combatState = card.CombatState ?? card.Owner.Creature.CombatState;
            PreviewValue = Hook.ModifyDamage(
                card.Owner.RunState,
                combatState,
                target,
                card.Owner.Creature,
                previewDamage,
                Props,
                card,
                ModifyDamageHookType.All,
                previewMode,
                out IEnumerable<AbstractModel> _);
        }
        else if (!card.IsEnchantmentPreview)
        {
            if (enchantment != null)
            {
                previewDamage += enchantment.EnchantDamageAdditive(previewDamage, Props);
                previewDamage *= enchantment.EnchantDamageMultiplicative(previewDamage, Props);
            }

            PreviewValue = previewDamage;
        }

        PreviewValue = Math.Max(PreviewValue, 0m);
    }
}
