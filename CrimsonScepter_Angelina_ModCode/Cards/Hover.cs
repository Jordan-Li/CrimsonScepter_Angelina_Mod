using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：悬停
/// 卡牌类型：技能牌
/// 稀有度：普通
/// 费用：1费
/// 效果：获得临时飞行，并抽牌
/// 升级后效果：临时飞行层数提高
/// 备注：基础飞行节奏牌
/// </summary>
public sealed class Hover : AngelinaCard
{
    // 额外悬浮说明：临时飞行
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<TemporaryFlyPower>()
    };

    // 动态变量：
    // 1. 临时飞行层数
    // 2. 抽牌数
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<TemporaryFlyPower>(1m),
        new CardsVar(2)
    };

    // 费用：1费，类型：技能牌，稀有度：普通，目标：自己
    public Hover()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    // 打出时的效果
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 第一步：获得临时飞行
        await PowerCmd.Apply<TemporaryFlyPower>(
            base.Owner.Creature,
            base.DynamicVars["TemporaryFlyPower"].BaseValue,
            base.Owner.Creature,
            this
        );

        // 第二步：抽牌
        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
    }

    // 升级后临时飞行 +1
    protected override void OnUpgrade()
    {
        base.DynamicVars["TemporaryFlyPower"].UpgradeValueBy(1m);
    }
}