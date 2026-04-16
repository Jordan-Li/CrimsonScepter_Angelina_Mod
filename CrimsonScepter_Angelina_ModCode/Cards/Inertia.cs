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
/// 卡牌名：Inertia
/// 卡牌类型：能力牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：当敌人进入失重时，或在抽牌前若场上已有失重敌人，额外抽牌。
/// 升级后效果：额外抽牌数提高1。
/// 备注：旧项目依赖旧版 ImbalanceStatePower；这里适配到现版本的 WeightlessPower。
/// </summary>
public sealed class Inertia : AngelinaCard
{
    // 额外悬浮说明：
    // 1. 惯性
    // 2. 失重
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<InertiaPower>(),
        HoverTipFactory.FromPower<WeightlessPower>()
    };

    // 动态变量：惯性提供的额外抽牌数
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<InertiaPower>(2m)
    };

    // 费用：1费，类型：能力牌，稀有度：非凡，目标：自己
    public Inertia()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 打出时的效果：获得惯性
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<InertiaPower>(
            base.Owner.Creature,
            base.DynamicVars["InertiaPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    // 升级后额外抽牌数 +1
    protected override void OnUpgrade()
    {
        base.DynamicVars["InertiaPower"].UpgradeValueBy(1m);
    }
}
