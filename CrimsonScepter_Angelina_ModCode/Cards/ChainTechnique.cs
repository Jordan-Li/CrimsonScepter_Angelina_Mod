using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：连锁技艺
/// 卡牌类型：技能牌
/// 稀有度：普通
/// 费用：1费
/// 效果：使目标获得失衡值。获得临时集中。若此牌使目标进入失重，改为获得集中。
/// 升级后效果：失衡值、临时集中和集中都提高。
/// </summary>
public sealed class ChainTechnique : AngelinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new IntVar("ImbalancePower", 10m),
        new IntVar("HotfixPower", 1m),
        new IntVar("FocusPower", 1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ImbalancePower>(),
        HoverTipFactory.FromPower<WeightlessPower>(),
        HoverTipFactory.FromPower<FocusPower>(),
        HoverTipFactory.FromPower<ChantTemporaryFocusNextTurnPower>()
    };

    public ChainTechnique()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        bool wasWeightless = cardPlay.Target.GetPower<WeightlessPower>() != null;

        await PowerCmd.Apply<ImbalancePower>(
            cardPlay.Target,
            base.DynamicVars["ImbalancePower"].BaseValue,
            base.Owner.Creature,
            this
        );

        bool isWeightless = cardPlay.Target.GetPower<WeightlessPower>() != null;
        if (!wasWeightless && isWeightless)
        {
            await PowerCmd.Apply<FocusPower>(
                base.Owner.Creature,
                base.DynamicVars["FocusPower"].BaseValue,
                base.Owner.Creature,
                this
            );
            return;
        }

        await PowerCmd.Apply<ChantTemporaryFocusNextTurnPower>(
            base.Owner.Creature,
            base.DynamicVars["HotfixPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ImbalancePower"].UpgradeValueBy(3m);
        base.DynamicVars["HotfixPower"].UpgradeValueBy(1m);
        base.DynamicVars["FocusPower"].UpgradeValueBy(1m);
    }
}