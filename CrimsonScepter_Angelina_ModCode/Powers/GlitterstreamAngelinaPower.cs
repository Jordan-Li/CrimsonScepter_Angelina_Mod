using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

public sealed class GlitterstreamAngelinaPower : AngelinaPower
{
    private sealed class Data
    {
        public int TriggerCountThisTurn { get; set; }
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool IsInstanced => false;

    public override bool ShouldScaleInMultiplayer => false;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (oldPileType != PileType.Exhaust ||
            card.Pile?.Type != PileType.Hand ||
            source is not DeliveryPower)
        {
            return Task.CompletedTask;
        }

        Data data = GetInternalData<Data>();
        if (data.TriggerCountThisTurn >= base.Amount)
        {
            return Task.CompletedTask;
        }

        data.TriggerCountThisTurn++;

        Glam glam = ModelDb.Enchantment<Glam>();
        if (glam.CanEnchant(card))
        {
            CardCmd.Enchant<Glam>(card, 1m);
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        _ = choiceContext;
        _ = combatState;

        if (side == base.Owner.Side)
        {
            GetInternalData<Data>().TriggerCountThisTurn = 0;
        }

        return Task.CompletedTask;
    }
}
