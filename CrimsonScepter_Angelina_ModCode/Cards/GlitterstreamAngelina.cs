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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class GlitterstreamAngelina : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? [CardKeyword.Innate]
        : [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? [
            HoverTipFactory.FromKeyword(CardKeyword.Innate),
            HoverTipFactory.FromPower<DeliveryPower>(),
            ..HoverTipFactory.FromEnchantment<Glam>(),
            HoverTipFactory.FromPower<GlitterstreamAngelinaPower>()
        ]
        : [
            HoverTipFactory.FromPower<DeliveryPower>(),
            ..HoverTipFactory.FromEnchantment<Glam>(),
            HoverTipFactory.FromPower<GlitterstreamAngelinaPower>()
        ];

    public GlitterstreamAngelina()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        List<CardModel> handCards = PileType.Hand.GetPile(base.Owner).Cards.ToList();
        if (handCards.Count > 0)
        {
            CardModel? selectedCard = (await CardSelectCmd.FromHand(
                context: choiceContext,
                player: base.Owner,
                prefs: new CardSelectorPrefs(new LocString("cards", "GLITTERSTREAM_ANGELINA.selectPrompt"), 1),
                filter: null,
                source: this)).FirstOrDefault();

            if (selectedCard is not null)
            {
                DeliveryPower? deliveryPower = base.Owner.Creature.GetPower<DeliveryPower>();
                deliveryPower ??= await PowerCmd.Apply<DeliveryPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);

                await CardCmd.Exhaust(choiceContext, selectedCard);

                if (deliveryPower != null)
                {
                    await deliveryPower.EnqueueCard(selectedCard);
                }
            }
        }

        await PowerCmd.Apply<GlitterstreamAngelinaPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
