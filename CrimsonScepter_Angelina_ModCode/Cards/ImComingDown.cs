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
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：我下来啦！
/// 卡牌类型：攻击牌
/// 稀有度：稀有
/// 费用：1费
/// 效果：失去所有飞行。每失去1层飞行，对所有敌人造成伤害。
/// 升级后效果：提高单段伤害。
/// </summary>
public sealed class ImComingDown : AngelinaCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<FlyPower>()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(20m, ValueProp.Move)
    };

    public ImComingDown()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FlyPower? flyPower = base.Owner.Creature.GetPower<FlyPower>();
        int lostFly = flyPower?.Amount ?? 0;
        if (lostFly <= 0)
        {
            return;
        }

        await PowerCmd.Remove(flyPower!);

        var combatState = base.CombatState ?? throw new InvalidOperationException("CombatState is null during ImComingDown.OnPlay.");

        for (int i = 0; i < lostFly; i++)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(combatState)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(5m);
    }
}