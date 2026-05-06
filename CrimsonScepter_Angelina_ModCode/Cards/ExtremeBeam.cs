using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class ExtremeBeam : DeliveredCardModel
{
    private const int BaseDamage = 5;

    private const int BaseCost = 0;

    private int currentDamage = BaseDamage;

    public override bool IsSpell => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => WithDeliveredTip(
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description")),
        HoverTipFactory.ForEnergy(this));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Unpowered | ValueProp.Move),
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered | ValueProp.Move)
            .WithMultiplier(static (card, _) => card.Owner?.Creature?.GetPower<FocusPower>()?.Amount ?? 0m),
        new EnergyVar(1)
    ];

    public ExtremeBeam()
        : base(BaseCost, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (base.CombatState == null)
        {
            return;
        }

        decimal spellDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue);
        await SpellHelper.DamageAll(
            choiceContext,
            base.Owner.Creature,
            base.CombatState.HittableEnemies,
            spellDamage,
            this);

        currentDamage *= 2;
        RefreshDisplayedState();

        base.EnergyCost.AddThisCombat(base.DynamicVars.Energy.IntValue);
        base.InvokeEnergyCostChanged();
        NCard.FindOnTable(this)?.PlayRandomizeCostAnim();
    }

    protected override Task OnDelivered(DeliveryPower deliveryPower)
    {
        _ = deliveryPower;
        ResetCombatState();
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStartLate()
    {
        ResetCombatState();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        ResetCombatState();
        return Task.CompletedTask;
    }

    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        if (!IsUpgraded || resultPileType != PileType.Discard)
        {
            return resultPileType;
        }

        return PileType.Hand;
    }

    protected override void OnUpgrade()
    {
    }

    private void ResetCombatState()
    {
        currentDamage = BaseDamage;
        base.EnergyCost.SetThisCombat(BaseCost);
        RefreshDisplayedState();
        base.InvokeEnergyCostChanged();
    }

    private void RefreshDisplayedState()
    {
        base.DynamicVars.Damage.BaseValue = currentDamage;
        base.DynamicVars.CalculationBase.BaseValue = currentDamage;
    }
}
