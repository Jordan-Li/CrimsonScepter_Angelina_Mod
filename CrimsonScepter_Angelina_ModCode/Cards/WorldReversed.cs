using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：世界倒转
/// 费用：2
/// 稀有度：稀有
/// 卡牌类型：技能
/// 效果：依次打出所有寄送牌，然后寄送当前手牌。
/// 升级后效果：费用变为1。
/// </summary>
public sealed class WorldReversed : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<DeliveryPower>()
    ];

    public WorldReversed()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        DeliveryPower? deliveryPower = base.Owner.Creature.GetPower<DeliveryPower>();
        if (deliveryPower != null)
        {
            List<CardModel> queuedCards = deliveryPower.GetQueuedCards().ToList();
            foreach (CardModel queuedCard in queuedCards)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    break;
                }

                CardModel? deliveredCard = await deliveryPower.DeliverCardNow(queuedCard);
                if (deliveredCard == null)
                {
                    continue;
                }

                await CardCmd.AutoPlay(choiceContext, deliveredCard, null, skipXCapture: true);
            }
        }

        List<CardModel> handCards = PileType.Hand.GetPile(base.Owner).Cards.ToList();
        if (handCards.Count == 0)
        {
            return;
        }

        deliveryPower = base.Owner.Creature.GetPower<DeliveryPower>();
        deliveryPower ??= await PowerCmd.Apply<DeliveryPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        if (deliveryPower == null)
        {
            return;
        }

        foreach (CardModel handCard in handCards)
        {
            await CardCmd.Exhaust(choiceContext, handCard);
            await deliveryPower.EnqueueCard(handCard);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
