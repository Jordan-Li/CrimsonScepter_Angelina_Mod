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

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：特限重力模块
/// 卡牌类型：能力牌
/// 稀有度：远古
/// 费用：1费
/// 效果：每当你打出一张攻击牌时，若其目标没有飞行，则给予其1层飞行。每打出一张牌，处于飞行状态的敌人受到2点伤害。
/// 升级后效果：获得固有。
/// </summary>
public sealed class SpecialGravityModule : AngelinaCard
{
    // 定义动态变量：给予的飞行层数、Power层数
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FlyPower>(1m),
        new PowerVar<SpecialGravityModulePower>(1m)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? new CardKeyword[] { CardKeyword.Innate }
        : Array.Empty<CardKeyword>();

    // 提示：显示固有、飞行和对应 Power
    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Innate),
            HoverTipFactory.FromPower<SpecialGravityModulePower>(),
            HoverTipFactory.FromPower<FlyPower>()
        }
        : new IHoverTip[]
        {
            HoverTipFactory.FromPower<SpecialGravityModulePower>(),
            HoverTipFactory.FromPower<FlyPower>()
        };

    // 费用：1费，类型：能力牌，稀有度：远古，自身目标
    public SpecialGravityModule()
        : base(1, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    // 打出时：施加特限重力模块 Power
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SpecialGravityModulePower>(
            base.Owner.Creature,
            base.DynamicVars["SpecialGravityModulePower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    // 升级后获得固有
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}