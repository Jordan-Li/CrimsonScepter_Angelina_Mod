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
/// 卡牌名：RealityManipulation
/// 卡牌类型：能力牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：每当你的 Exhaust 数量发生变化时，获得格挡。
/// 升级后效果：每次触发获得的格挡提高1。
/// 备注：旧项目真实存在卡牌，按旧逻辑迁移。
/// </summary>
public sealed class RealityManipulation : AngelinaCard
{
    // 额外悬浮说明：现实操纵
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<RealityManipulationPower>()
    };

    // 动态变量：现实操纵每次触发给予的格挡值
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<RealityManipulationPower>(1m)
    };

    // 费用：1费，类型：能力牌，稀有度：非凡，目标：自己
    public RealityManipulation()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 打出时的效果：获得现实操纵
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<RealityManipulationPower>(
            base.Owner.Creature,
            base.DynamicVars["RealityManipulationPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    // 升级后每次触发获得的格挡值 +1
    protected override void OnUpgrade()
    {
        base.DynamicVars["RealityManipulationPower"].UpgradeValueBy(1m);
    }
}
