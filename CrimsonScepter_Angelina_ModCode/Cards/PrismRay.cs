using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：棱镜射线
/// 卡牌类型：攻击牌
/// 稀有度：稀有
/// 费用：1费
/// 效果：造成法术伤害。变化所有寄送的牌。
/// 升级后效果：变化所有寄送的牌并将其升级。
/// </summary>
public sealed class PrismRay : AngelinaCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DeliveryPower>(),
        HoverTipFactory.Static(StaticHoverTip.Transform),
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description"))
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Unpowered | ValueProp.Move)
    };

    public PrismRay()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        decimal damage = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue);
        await SpellHelper.Damage(choiceContext, base.Owner.Creature, cardPlay.Target, damage, this);

        await TransformDeliveredCards();
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        decimal displayedDamage = base.DynamicVars.Damage.BaseValue;
        if (base.IsMutable && base.Owner?.Creature != null)
        {
            displayedDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, displayedDamage);
        }

        description.Add("DisplayedDamage", displayedDamage);
    }

    private async Task TransformDeliveredCards()
    {
        DeliveryPower? deliveryPower = base.Owner.Creature.GetPower<DeliveryPower>();
        if (deliveryPower == null)
        {
            return;
        }

        List<CardModel> queuedCards = deliveryPower.GetQueuedCards().ToList();
        if (queuedCards.Count == 0)
        {
            return;
        }

        foreach (CardModel queuedCard in queuedCards)
        {
            if (queuedCard.Pile?.Type != PileType.Exhaust || !queuedCard.IsTransformable)
            {
                continue;
            }

            CardPileAddResult result = await CardCmd.TransformToRandom(
                queuedCard,
                base.Owner.RunState.Rng.Niche,
                CardPreviewStyle.HorizontalLayout);

            CardModel transformedCard = result.cardAdded;

            if (base.IsUpgraded && transformedCard.IsUpgradable)
            {
                CardCmd.Upgrade(transformedCard, CardPreviewStyle.None);
            }

            await deliveryPower.SetSelectedCard(transformedCard);
        }
    }
}