using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class StellarResonance : AngelinaCard
{
    private const decimal BaseDamageAmount = 7m;
    private const decimal BonusPerEnchantedCard = 5m;

    public override bool IsSpell => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description"))
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseDamageAmount),
        new ExtraDamageVar(BonusPerEnchantedCard),
        new SpellCalculatedDamageVar(ValueProp.Unpowered | ValueProp.Move)
            .WithMultiplier(static (card, _) => CountEnchantedCards(card.Owner?.PlayerCombatState))
    ];

    public StellarResonance()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        decimal baseDamage = base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        decimal spellDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, baseDamage);
        await SpellHelper.Damage(choiceContext, base.Owner.Creature, cardPlay.Target, spellDamage, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.CalculationBase.UpgradeValueBy(2m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }

    private static int CountEnchantedCards(PlayerCombatState? playerCombatState)
    {
        return playerCombatState?.AllCards.Count(card => card.Enchantment != null) ?? 0;
    }
}
