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

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：小戏法
/// 卡牌类型：技能牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：从手牌中选择1张其他牌，复制该牌，并将复制品寄送。
/// 升级后效果：费用减少1点。
/// 备注：这张牌本质上是“寄送一个复制品”，不会把原牌送走。
/// </summary>
public sealed class LittleTrick : AngelinaCard
{
    // 额外悬浮说明：寄送
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DeliveryPower>()
    };

    // 费用：1费，类型：技能牌，稀有度：非凡，目标：自己
    public LittleTrick()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 打出时的效果
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 第一步：从手牌中选择1张牌
        CardModel? selectedCard = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(new LocString("cards", "LITTLE_TRICK.selectPrompt"), 1),
            filter: card => card != this,
            source: this)).FirstOrDefault();

        if (selectedCard == null)
        {
            return;
        }

        // 第二步：复制该牌
        CardModel copy = selectedCard.CreateClone();

        // 第三步：把复制品直接生成到 Exhaust
        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(
                copy,
                PileType.Exhaust,
                addedByPlayer: true));

        // 第四步：加入寄送队列
        DeliveryPower? deliveryPower = base.Owner.Creature.GetPower<DeliveryPower>();
        deliveryPower ??= await PowerCmd.Apply<DeliveryPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);

        if (deliveryPower != null)
        {
            await deliveryPower.SetSelectedCard(copy);
        }
    }

    // 升级后减1费
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}