using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class TagRelay : AngelinaCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DeliveryPower>()
    ];

    public TagRelay()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
    {
    }

    protected override (PileType, CardPilePosition) GetResultPileTypeAndPositionForCardPlay()
    {
        return (PileType.None, CardPilePosition.Bottom);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        Player targetPlayer = cardPlay.Target.Player ?? throw new InvalidOperationException("Target player is null during TagRelay.OnPlay.");
        Creature targetCreature = cardPlay.Target;
        ICombatState combatState = base.CombatState ?? throw new InvalidOperationException("CombatState is null during TagRelay.OnPlay.");

        DeliveryPower? targetDeliveryPower = targetCreature.GetPower<DeliveryPower>();
        targetDeliveryPower ??= await PowerCmd.Apply<DeliveryPower>(choiceContext, targetCreature, 1m, base.Owner.Creature, this);
        if (targetDeliveryPower == null)
        {
            return;
        }

        await QueueCopyForTarget(this, targetPlayer, targetDeliveryPower, combatState);

        List<CardModel> handCards = (base.Owner.PlayerCombatState?.Hand.Cards ?? []).ToList();
        int maxSelectable = Math.Min(2, handCards.Count);
        if (maxSelectable <= 0)
        {
            return;
        }

        List<CardModel> selectedCards = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(new LocString("cards", "TAG_RELAY.selectPrompt"), 0, maxSelectable),
            filter: null,
            source: this)).ToList();

        foreach (CardModel selectedCard in selectedCards)
        {
            await QueueCopyForTarget(selectedCard, targetPlayer, targetDeliveryPower, combatState);
            await CardPileCmd.RemoveFromCombat(selectedCard);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.SetCustomBaseCost(0);
    }

    private static async Task QueueCopyForTarget(CardModel sourceCard, Player targetPlayer, DeliveryPower targetDeliveryPower, ICombatState combatState)
    {
        CardModel copiedCard = CardModel.FromSerializable(sourceCard.ToSerializable());
        combatState.AddCard(copiedCard, targetPlayer);
        await CardPileCmd.AddGeneratedCardToCombat(copiedCard, PileType.Exhaust, sourceCard.Owner);
        await targetDeliveryPower.EnqueueCard(copiedCard);
    }
}
