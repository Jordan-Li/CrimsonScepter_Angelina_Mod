using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：兼职工作
/// 效果：回合结束时，若本回合没有打出过攻击牌，则恢复生命值，然后移除自身。
/// </summary>
public sealed class PartTimeJobPower : AngelinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != base.Owner.Side || base.Owner.Player == null || base.CombatState == null)
        {
            return;
        }

        bool playedAttackThisTurn = CombatManager.Instance.History.CardPlaysStarted.Any(entry =>
            entry.HappenedThisTurn(base.CombatState) &&
            entry.CardPlay.Card.Owner == base.Owner.Player &&
            entry.CardPlay.Card.Type == CardType.Attack);

        if (!playedAttackThisTurn)
        {
            Flash();
            await CreatureCmd.Heal(base.Owner, base.Amount);
        }

        await PowerCmd.Remove(this);
    }
}