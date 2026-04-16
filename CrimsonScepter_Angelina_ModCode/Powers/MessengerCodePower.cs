using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：信使准则
/// 效果：每回合额外抽1张牌，然后寄送1张牌。
/// </summary>
public sealed class MessengerCodePower : AngelinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DeliveryPower>()
    };

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner.Player)
        {
            return count;
        }

        return count + 1m;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        CardModel? selectedCard = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: player,
            prefs: new CardSelectorPrefs(new LocString("cards", "MESSENGER_CODE.selectPrompt"), 1),
            filter: _ => true,
            source: null!)).FirstOrDefault();

        if (selectedCard is null)
        {
            return;
        }

        Flash();

        DeliveryPower? deliveryPower = base.Owner.GetPower<DeliveryPower>();
        deliveryPower ??= await PowerCmd.Apply<DeliveryPower>(base.Owner, 1m, base.Owner, null!);

        if (deliveryPower is null)
        {
            return;
        }

        await CardCmd.Exhaust(choiceContext, selectedCard);
        await deliveryPower.SetSelectedCard(selectedCard);
    }
}