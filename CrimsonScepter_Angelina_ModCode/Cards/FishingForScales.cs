using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class FishingForScales : AngelinaCard
{
    public override TargetType TargetType => IsUpgraded
        ? TargetType.AnyEnemy
        : TargetType.RandomEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new PowerVar<ImbalancePower>(12m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ImbalancePower>()
    ];

    public FishingForScales()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(base.CombatState, nameof(base.CombatState));

        AttackCommand attackCommand = DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitFx("vfx/vfx_flying_slash");

        if (IsUpgraded)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            attackCommand = attackCommand.Targeting(cardPlay.Target);
        }
        else
        {
            attackCommand = attackCommand.TargetingRandomOpponents(base.CombatState);
        }

        AttackCommand attackResult = await attackCommand.Execute(choiceContext);
        if (attackResult.Results.Any(result => result.UnblockedDamage > 0))
        {
            return;
        }

        List<Creature> enemies = base.CombatState
            .GetOpponentsOf(base.Owner.Creature)
            .Where(enemy => enemy.IsAlive && enemy.IsHittable)
            .ToList();

        if (enemies.Count == 0)
        {
            return;
        }

        await PowerCmd.Apply<ImbalancePower>(enemies, base.DynamicVars["ImbalancePower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ImbalancePower"].UpgradeValueBy(6m);
    }
}
