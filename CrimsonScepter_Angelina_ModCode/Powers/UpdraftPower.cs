using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：上升气流
/// 效果：
/// 1. 自身回合开始时，获得等量临时飞行
/// 2. 若自身因受到攻击而解除浮空，向抽牌堆加入一张眩晕
/// </summary>
public sealed class UpdraftPower : AngelinaPower
{
    private sealed class Data
    {
        public bool PendingAttackGroundedCheck;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<TemporaryFlyPower>(),
        HoverTipFactory.FromCard<Dazed>()
    };

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return;
        }

        await PowerCmd.Apply<TemporaryFlyPower>(base.Owner, base.Amount, base.Owner, null);
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner || result.UnblockedDamage <= 0 || !IsPoweredAttack(props))
        {
            return Task.CompletedTask;
        }

        if (TemporaryFlyPower.IsResolvingExpiration)
        {
            return Task.CompletedTask;
        }

        if ((base.Owner.GetPower<FlyPower>()?.Amount ?? 0) <= 0)
        {
            return Task.CompletedTask;
        }

        GetInternalData<Data>().PendingAttackGroundedCheck = true;
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        Data data = GetInternalData<Data>();
        if (!data.PendingAttackGroundedCheck)
        {
            return;
        }

        if (!CombatManager.Instance.IsInProgress || TemporaryFlyPower.IsResolvingExpiration)
        {
            data.PendingAttackGroundedCheck = false;
            return;
        }

        if (power.Owner != base.Owner || power is not FlyPower || !AirborneHelper.BecameGroundedByFlyChange(power.Owner, amount))
        {
            return;
        }

        data.PendingAttackGroundedCheck = false;
        if (base.Owner.Player == null)
        {
            return;
        }

        CardModel dazed = base.CombatState.CreateCard<Dazed>(base.Owner.Player);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(dazed, PileType.Draw, addedByPlayer: true));
        Flash();
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}