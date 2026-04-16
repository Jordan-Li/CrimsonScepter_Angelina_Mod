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
/// 卡牌名：水平线
/// 卡牌类型：技能牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：使所有单位获得飞行。
/// 未升级时带消耗。
/// 升级后效果：移除消耗。
/// </summary>
public sealed class HorizonLine : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? Array.Empty<CardKeyword>()
        : new CardKeyword[]
        {
            CardKeyword.Exhaust
        };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
        {
            HoverTipFactory.FromPower<FlyPower>()
        }
        : new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
            HoverTipFactory.FromPower<FlyPower>()
        };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FlyPower>(1m)
    };

    public HorizonLine()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = base.CombatState ?? throw new InvalidOperationException("CombatState is null during HorizonLine.OnPlay.");

        await PowerCmd.Apply<FlyPower>(
            combatState.Creatures,
            base.DynamicVars["FlyPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}