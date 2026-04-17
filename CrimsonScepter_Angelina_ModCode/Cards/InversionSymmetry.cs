using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：反演对称
/// 卡牌类型：技能牌
/// 稀有度：稀有
/// 费用：X费
/// 效果：将弃牌堆中最近的X张其他牌依次打出。
/// 升级后效果：获得保留。
/// </summary>
public sealed class InversionSymmetry : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? new CardKeyword[] { CardKeyword.Retain }
        : Array.Empty<CardKeyword>();

    protected override bool HasEnergyCostX => true;

    public InversionSymmetry()
        : base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 结算本次 X 费实际消耗
        int spentEnergy = ResolveEnergyXValue();
        if (spentEnergy <= 0)
        {
            return;
        }

        // 只查看弃牌堆顶往下的前 X 张牌，并按从上到下的顺序处理。
        CardPile discardPile = PileType.Discard.GetPile(base.Owner);
        List<CardModel> topCards = discardPile.Cards
            .Reverse()
            .Where(card => card is not InversionSymmetry)
            .Take(spentEnergy)
            .ToList();

        // 同名牌只打出第一张，不向更深处补位。
        List<CardModel> cards = topCards
            .DistinctBy(card => card.Id)
            .ToList();

        // 依次打出这些牌
        foreach (CardModel card in cards)
        {
            if (CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }

            await CardCmd.AutoPlay(choiceContext, card, null, skipXCapture: true);
        }

        // 未能实际用于重放的 X 费在结算后返还。
        int refundedEnergy = spentEnergy - cards.Count;
        if (refundedEnergy > 0)
        {
            await PlayerCmd.GainEnergy(refundedEnergy, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
