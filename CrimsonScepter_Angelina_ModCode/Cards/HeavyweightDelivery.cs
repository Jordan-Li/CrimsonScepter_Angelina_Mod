using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class HeavyweightDelivery : DeliveredCardModel
{
    private bool _doubleImbalanceThisTurn;

    protected override bool HasEnergyCostX => true;

    protected override bool ShouldGlowGoldInternal => _doubleImbalanceThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ImbalancePower>(7m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => WithDeliveredTip(
        HoverTipFactory.FromPower<ImbalancePower>());

    public HeavyweightDelivery()
        : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        int triggerCount = ResolveEnergyXValue();
        if (triggerCount <= 0)
        {
            return;
        }

        decimal imbalanceAmount = base.DynamicVars["ImbalancePower"].BaseValue;
        if (_doubleImbalanceThisTurn)
        {
            imbalanceAmount *= 2m;
        }

        List<Creature> enemies = (base.CombatState?.HittableEnemies ?? Enumerable.Empty<Creature>())
            .Where(enemy => enemy.IsAlive)
            .ToList();

        for (int i = 0; i < triggerCount; i++)
        {
            foreach (Creature enemy in enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                await PowerCmd.Apply<ImbalancePower>(enemy, imbalanceAmount, base.Owner.Creature, this);
            }
        }
    }

    protected override Task OnDelivered(DeliveryPower deliveryPower)
    {
        _ = deliveryPower;
        _doubleImbalanceThisTurn = true;
        return Task.CompletedTask;
    }

    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _ = choiceContext;

        if (side == base.Owner.Creature.Side)
        {
            _doubleImbalanceThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        _doubleImbalanceThisTurn = false;
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ImbalancePower"].UpgradeValueBy(3m);
    }
}
