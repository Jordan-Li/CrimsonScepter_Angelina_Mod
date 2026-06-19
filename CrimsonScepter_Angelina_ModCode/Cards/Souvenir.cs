using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class Souvenir : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Souvenir()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        List<CardModel> options = CardFactory.GetDistinctForCombat(
                base.Owner,
                GetOtherCharacterRareCards(),
                3,
                base.Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        if (options.Count == 0)
        {
            return;
        }

        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(options, CardPreviewStyle.None);
        }

        CardModel? selectedCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, base.Owner);
        if (selectedCard == null)
        {
            return;
        }

        selectedCard.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, base.Owner);
    }

    protected override void OnUpgrade()
    {
    }

    private IEnumerable<CardModel> GetOtherCharacterRareCards()
    {
        return GetOtherCharacterPools()
            .SelectMany(pool => pool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
            .Where(card =>
                card.Rarity == CardRarity.Rare &&
                card.CanBeGeneratedInCombat &&
                card.Pool is not AngelinaCardPool);
    }

    private static IEnumerable<CardPoolModel> GetOtherCharacterPools()
    {
        yield return ModelDb.CardPool<IroncladCardPool>();
        yield return ModelDb.CardPool<SilentCardPool>();
        yield return ModelDb.CardPool<DefectCardPool>();
        yield return ModelDb.CardPool<NecrobinderCardPool>();
        yield return ModelDb.CardPool<RegentCardPool>();
    }
}
