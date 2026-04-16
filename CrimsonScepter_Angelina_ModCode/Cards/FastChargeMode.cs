using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：FastChargeMode
/// 卡牌类型：技能牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：本回合内，你施加的失衡翻倍。
/// 升级后效果：费用减少1点。
/// 备注：旧项目真实存在卡牌，按旧逻辑迁移。
/// </summary>
public sealed class FastChargeMode : AngelinaCard
{
    // 额外悬浮说明：
    // 1. 快速充能模式
    // 2. 失衡
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<FastChargeModePower>(),
        HoverTipFactory.FromPower<ImbalancePower>()
    };

    // 费用：1费，类型：技能牌，稀有度：非凡，目标：自己
    public FastChargeMode()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 打出时的效果：获得本回合的“快速充能模式”
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FastChargeModePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    // 升级后减1费
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
