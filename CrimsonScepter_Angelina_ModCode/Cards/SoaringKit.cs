using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：腾空套组
/// 卡牌类型：技能牌
/// 稀有度：普通
/// 费用：0费
/// 效果：给予自己飞行。
/// 送达时：给予所有敌方1层飞行。
/// 升级后效果：提高自身获得的飞行层数。
/// </summary>
public sealed class SoaringKit : DeliveredCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Exhaust
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => WithDeliveredTip(
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<FlyPower>(),
        HoverTipFactory.FromPower<DeliveryPower>()
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FlyPower>(1m)
    };

    public SoaringKit()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FlyPower>(
            base.Owner.Creature,
            base.DynamicVars["FlyPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["FlyPower"].UpgradeValueBy(1m);
    }

    protected override async Task OnDelivered(DeliveryPower deliveryPower)
    {
        var combatState = base.CombatState ?? throw new InvalidOperationException("CombatState is null during SoaringKit.OnDelivered.");

        foreach (Creature enemy in combatState.HittableEnemies)
        {
            await PowerCmd.Apply<FlyPower>(enemy, 1m, base.Owner.Creature, this);
        }
    }
}