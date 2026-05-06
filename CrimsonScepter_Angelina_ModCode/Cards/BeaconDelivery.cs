using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class BeaconDelivery : DeliveredCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => WithDeliveredTip(
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<DeliveryPower>());

    public BeaconDelivery()
        : base(-1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        return Task.CompletedTask;
    }

    protected override async Task OnDelivered(DeliveryPower deliveryPower)
    {
        _ = deliveryPower;

        await CardPileCmd.Add(this, PileType.Exhaust, source: this);

        CardPile drawPile = PileType.Draw.GetPile(base.Owner);
        int maxSelectCount = System.Math.Min(base.DynamicVars.Cards.IntValue, drawPile.Cards.Count);
        if (maxSelectCount == 0)
        {
            return;
        }

        List<CardModel> cardsToAdd = (await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            drawPile.Cards,
            base.Owner,
            new CardSelectorPrefs(new LocString("cards", "BEACON_DELIVERY.selectPrompt"), 0, maxSelectCount)))
            .ToList();

        if (cardsToAdd.Count == 0)
        {
            return;
        }

        await CardPileCmd.Add(cardsToAdd, PileType.Hand, source: this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
