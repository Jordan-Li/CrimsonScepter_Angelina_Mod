using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

public sealed class MatrixDisplacementPower : AngelinaPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description")),
        HoverTipFactory.FromPower<DeliveryPower>()
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        _ = props;

        if (target != base.Owner ||
            dealer == null ||
            dealer.Side == base.Owner.Side ||
            cardSource == null ||
            !SpellHelper.IsSpell(cardSource) ||
            result.UnblockedDamage <= 0)
        {
            return;
        }

        Player? player = dealer.Player;
        if (player == null)
        {
            return;
        }

        Flash();
        for (int i = 0; i < base.Amount; i++)
        {
            await CardPileCmd.Draw(choiceContext, 1m, player);
            await SendOneCardFromHand(choiceContext, player, dealer);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        _ = participants;
        if (side != base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }

    private async Task SendOneCardFromHand(PlayerChoiceContext choiceContext, Player player, Creature dealer)
    {
        List<CardModel> handCards = PileType.Hand.GetPile(player).Cards.ToList();
        if (handCards.Count == 0)
        {
            return;
        }

        CardModel? selectedCard = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: player,
            prefs: new CardSelectorPrefs(new LocString("cards", "MATRIX_DISPLACEMENT.sendPrompt"), 1),
            filter: null,
            source: this)).FirstOrDefault();

        if (selectedCard is null)
        {
            return;
        }

        DeliveryPower? deliveryPower = dealer.GetPower<DeliveryPower>();
        deliveryPower ??= await PowerCmd.Apply<DeliveryPower>(choiceContext, dealer, 1m, dealer, null);

        await CardCmd.Exhaust(choiceContext, selectedCard);
        if (deliveryPower != null)
        {
            await deliveryPower.EnqueueCard(selectedCard);
        }
    }
}
