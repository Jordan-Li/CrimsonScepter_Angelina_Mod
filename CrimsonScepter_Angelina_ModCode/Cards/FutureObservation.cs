using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：未来观测
/// 卡牌类型：能力牌
/// 稀有度：稀有
/// 费用：2费
/// 效果：打出时，选择5张卡组中的牌，其会出现在你下次战斗的抽牌堆顶部。
/// 升级后效果：费用降低为1。
/// 备注：通过“祈愿星”遗物跨战斗保存选择结果。
/// </summary>
public sealed class FutureObservation : AngelinaCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[]
        {
            new HoverTip(
                new LocString("cards", "FUTURE_OBSERVATION.multiplayerWarningTitle"),
                new LocString("cards", "FUTURE_OBSERVATION.multiplayerWarningDescription"))
        }.Concat(HoverTipFactory.FromRelic<WishingStar>());

    protected override bool IsPlayable => base.Owner != null &&
                                          !base.Owner.Relics.Any(relic => relic.Id == ModelDb.Relic<WishingStar>().Id);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public FutureObservation()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.Owner.Relics.Any(relic => relic.Id == ModelDb.Relic<WishingStar>().Id))
        {
            return;
        }

        await RelicCmd.Obtain(ModelDb.Relic<WishingStar>().ToMutable(), base.Owner);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}