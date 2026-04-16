using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：快速派送
/// 卡牌类型：技能牌
/// 稀有度：普通
/// 费用：1费
/// 效果：随机选择1张已寄送的牌，将其立即送达。
/// 升级后效果：选择1张已寄送的牌，将其立即送达。
/// </summary>
public sealed class QuickDispatch : AngelinaCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DeliveryPower>()
    };

    protected override bool IsPlayable =>
        base.Owner?.Creature.GetPower<DeliveryPower>()?.GetQueuedCards().Count > 0;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public QuickDispatch()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        DeliveryPower? deliveryPower = base.Owner.Creature.GetPower<DeliveryPower>();
        if (deliveryPower == null || deliveryPower.GetQueuedCards().Count == 0)
        {
            return;
        }

        if (base.IsUpgraded)
        {
            await deliveryPower.DeliverChosen(choiceContext, this);
        }
        else
        {
            await deliveryPower.DeliverRandom(choiceContext, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}