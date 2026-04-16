using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：能量循环
/// 效果：每回合中，打出耗能至少为2的攻击、技能或能力牌时，每种类型至多各触发若干次：回复1点能量。
/// </summary>
public sealed class EnergyCyclePower : AngelinaPower
{
    private sealed class Data
    {
        public Dictionary<CardType, int> TriggerCountsByType { get; } = new();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    public override int DisplayAmount => base.Amount;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.ForEnergy(this)
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1)
    };

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }

        CardType type = cardPlay.Card.Type;
        if (type != CardType.Attack && type != CardType.Skill && type != CardType.Power)
        {
            return;
        }

        if (cardPlay.Resources.EnergyValue < 2)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.TriggerCountsByType.TryGetValue(type, out int triggerCount);
        if (triggerCount >= base.Amount)
        {
            return;
        }

        data.TriggerCountsByType[type] = triggerCount + 1;
        Flash();
        await PlayerCmd.GainEnergy(1m, base.Owner.Player);
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == base.Owner.Side)
        {
            GetInternalData<Data>().TriggerCountsByType.Clear();
        }

        return Task.CompletedTask;
    }
}