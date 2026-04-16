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
/// 卡牌名：DecelBinder
/// 卡牌类型：能力牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：每当你打出攻击牌，对目标施加层数等同于本能力的迟滞。
/// 升级后效果：变为固有。
/// 备注：旧项目真实存在卡牌，按旧逻辑迁移。
/// </summary>
public sealed class DecelBinder : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? new CardKeyword[] { CardKeyword.Innate }
        : System.Array.Empty<CardKeyword>();

    // 额外悬浮说明：
    // 1. 迟滞
    // 2. 减速束缚
    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Innate),
            HoverTipFactory.FromPower<StaggerPower>(),
            HoverTipFactory.FromPower<DecelBinderPower>()
        }
        : new IHoverTip[]
        {
            HoverTipFactory.FromPower<StaggerPower>(),
            HoverTipFactory.FromPower<DecelBinderPower>()
        };

    // 动态变量：减速束缚的层数
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DecelBinderPower>(1m)
    };

    // 费用：1费，类型：能力牌，稀有度：非凡，目标：自己
    public DecelBinder()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 打出时的效果：获得减速束缚
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DecelBinderPower>(
            base.Owner.Creature,
            base.DynamicVars["DecelBinderPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    // 升级后：变为固有
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
