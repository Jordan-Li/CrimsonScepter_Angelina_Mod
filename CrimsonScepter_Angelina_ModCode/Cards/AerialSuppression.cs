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
/// 卡牌名：浮空压制
/// 卡牌类型：技能牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：给予目标飞行，并使其本回合失去力量。
/// 升级后效果：提高失去力量的数值。
/// </summary>
public sealed class AerialSuppression : AngelinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FlyPower>(1m),
        new DynamicVar("StrengthLoss", 8m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<FlyPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    };

    public AerialSuppression()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await PowerCmd.Apply<FlyPower>(
            cardPlay.Target,
            base.DynamicVars["FlyPower"].BaseValue,
            base.Owner.Creature,
            this
        );

        await PowerCmd.Apply<PiercingWailPower>(
            cardPlay.Target,
            base.DynamicVars["StrengthLoss"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["StrengthLoss"].UpgradeValueBy(2m);
    }
}