using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：在下面！
/// 卡牌类型：攻击牌
/// 稀有度：普通
/// 费用：1费
/// 效果：若你不处于浮空，则对所有处于浮空的敌人施加虚弱和失衡值。然后造成伤害。
/// 若你处于浮空，则只造成伤害。
/// 升级后效果：提高伤害和虚弱层数。
/// </summary>
public sealed class DownHere : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Retain
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<ImbalancePower>(),
        HoverTipFactory.FromPower<FlyPower>(),
        new HoverTip(
            new LocString("powers", "AIRBORNE.title"),
            new LocString("powers", "AIRBORNE.description"))
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new PowerVar<ImbalancePower>(5m)
    };

    public DownHere()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        if (!AirborneHelper.IsAirborne(base.Owner.Creature))
        {
            var combatState = base.CombatState ?? throw new InvalidOperationException("CombatState is null during DownHere.OnPlay.");

            IEnumerable<Creature> airborneEnemies = combatState.HittableEnemies
                .Where(AirborneHelper.IsAirborne);

            foreach (Creature enemy in airborneEnemies)
            {
                await PowerCmd.Apply<WeakPower>(
                    enemy,
                    base.DynamicVars["WeakPower"].BaseValue,
                    base.Owner.Creature,
                    this
                );

                await PowerCmd.Apply<ImbalancePower>(
                    enemy,
                    base.DynamicVars["ImbalancePower"].BaseValue,
                    base.Owner.Creature,
                    this
                );
            }
        }

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars["WeakPower"].UpgradeValueBy(1m);
    }
}