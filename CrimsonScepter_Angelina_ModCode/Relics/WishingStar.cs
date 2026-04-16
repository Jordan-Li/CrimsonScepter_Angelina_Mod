using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Relics;

/// <summary>
/// 遗物名：祈愿星
/// 效果：获得时，选择5张牌库中的牌。
/// 下次战斗开始前，将这些牌（若仍在牌组中）按选择顺序置于抽牌堆顶，然后失去此遗物。
/// 备注：用于“未来观测”的跨战斗存储。
/// </summary>
public sealed class WishingStar : AngelinaRelic
{
    private const int MaxSelectedCards = 5;

    private List<SerializableCard> _selectedCards = new();

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    public override bool ShowCounter => base.IsMutable && _selectedCards.Count > 0;

    public override int DisplayAmount => base.IsMutable ? _selectedCards.Count : 0;

    [SavedProperty]
    public List<SerializableCard> SelectedCards
    {
        get => _selectedCards;
        private set
        {
            AssertMutable();
            _selectedCards.Clear();
            _selectedCards.AddRange(value);
            InvokeDisplayAmountChanged();
        }
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _selectedCards = new List<SerializableCard>();
    }

    // 获得时：从牌库中选择5张牌
    public override async Task AfterObtained()
    {
        List<CardModel> eligibleCards = base.Owner.Deck.Cards
            .Where(FilterSelectableCard)
            .ToList();

        if (eligibleCards.Count == 0)
        {
            await RelicCmd.Remove(this);
            return;
        }

        int selectCount = Math.Min(MaxSelectedCards, eligibleCards.Count);

        CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, selectCount, selectCount)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        List<CardModel> selectedCards = (await CardSelectCmd.FromDeckGeneric(
            prefs: prefs,
            player: base.Owner,
            filter: FilterSelectableCard)).ToList();

        SelectedCards = selectedCards
            .Select(card => card.ToSerializable())
            .ToList();

        Flash();
    }

    // 下次战斗开始前：把已选择的牌按顺序置于抽牌堆顶，然后移除自身
    public override async Task BeforeCombatStartLate()
    {
        if (SelectedCards.Count == 0)
        {
            await RelicCmd.Remove(this);
            return;
        }

        List<CardModel> orderedCombatCards = ResolveSelectedCombatCardsInOrder();

        // 为了让“第一个选中的牌”最终位于最顶端，
        // 这里需要倒序逐张放到抽牌堆顶。
        for (int i = orderedCombatCards.Count - 1; i >= 0; i--)
        {
            await CardPileCmd.Add(orderedCombatCards[i], PileType.Draw, CardPilePosition.Top, this);
        }

        Flash();
        SelectedCards = new List<SerializableCard>();
        await RelicCmd.Remove(this);
    }

    private bool FilterSelectableCard(CardModel card)
    {
        return card.Type != CardType.Quest;
    }

    private List<CardModel> ResolveSelectedCombatCardsInOrder()
    {
        List<CardModel> resolved = new();
        List<CardModel> drawPileCards = PileType.Draw.GetPile(base.Owner).Cards.ToList();

        foreach (SerializableCard selected in SelectedCards)
        {
            CardModel desiredDeckCard = CardModel.FromSerializable(selected);

            CardModel? matchedCombatCard = drawPileCards.FirstOrDefault(card =>
                card.DeckVersion != null &&
                ReferenceEquals(card.DeckVersion, FindMatchingDeckCard(desiredDeckCard)));

            if (matchedCombatCard == null)
            {
                continue;
            }

            resolved.Add(matchedCombatCard);
            drawPileCards.Remove(matchedCombatCard);
        }

        return resolved;
    }

    private CardModel? FindMatchingDeckCard(CardModel desiredDeckCard)
    {
        foreach (CardModel card in base.Owner.Deck.Cards)
        {
            if (card.Id != desiredDeckCard.Id)
            {
                continue;
            }

            if (card.IsUpgraded != desiredDeckCard.IsUpgraded)
            {
                continue;
            }

            return card;
        }

        return null;
    }
}