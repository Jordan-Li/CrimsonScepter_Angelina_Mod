using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

/// <summary>
/// 法术辅助工具：
/// 1. 法术伤害与法术格挡不吃力量、敏捷、易伤、虚弱、脆弱等常规修正
/// 2. 但会受到集中（Focus）的影响
/// 3. 这里只负责“法术”的数值结算，不把法术当作 Power
/// </summary>
public static class SpellHelper
{
    // 法术统一使用的 ValueProp：
    // - Unpowered：不走常规力量/敏捷等修正
    // - Move：保留为正常行动来源
    private const ValueProp SpellProps = ValueProp.Unpowered | ValueProp.Move;

    /// <summary>
    /// 获取法术额外修正值。
    /// 当前旧版逻辑只有：集中（Focus）会影响法术。
    /// </summary>
    public static decimal GetSpellBonus(Creature? source)
    {
        return source?.GetPower<FocusPower>()?.Amount ?? 0m;
    }

    /// <summary>
    /// 计算法术最终显示/结算值：基础值 + 集中，最低不小于0。
    /// </summary>
    public static decimal ModifySpellValue(Creature? source, decimal baseValue)
    {
        return decimal.Max(0m, baseValue + GetSpellBonus(source));
    }

    /// <summary>
    /// 按法术规则造成伤害。
    /// </summary>
    public static async Task Damage(
        PlayerChoiceContext choiceContext,
        Creature? source,
        Creature? target,
        decimal amount,
        CardModel? cardSource)
    {
        if (target == null || amount <= 0m)
        {
            return;
        }

        await CreatureCmd.Damage(
            choiceContext,
            target,
            amount,
            SpellProps,
            source,
            cardSource
        );
    }

    /// <summary>
    /// 按法术规则获得格挡。
    /// </summary>
    public static async Task<decimal> GainBlock(
        Creature? source,
        Creature? target,
        decimal amount,
        CardPlay cardPlay)
    {
        _ = source;

        if (target == null || amount <= 0m)
        {
            return 0m;
        }

        return await CreatureCmd.GainBlock(
            target,
            amount,
            SpellProps,
            cardPlay
        );
    }
}