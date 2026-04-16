using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：临时飞行
/// 原版语义：
/// 1. 施加时，立刻获得等量飞行
/// 2. 该Power只记录“下个自身回合开始时要失去多少层飞行”
/// 3. 不追踪中途飞行是否被消耗
/// 4. 到下个自身回合开始时，失去等量飞行，然后移除该Power
/// </summary>
public sealed class TemporaryFlyPower : AngelinaPower
{
    private bool _shouldIgnoreNextInstance;

    [System.ThreadStatic]
    private static bool _isResolvingExpiration;

    public static bool IsResolvingExpiration => _isResolvingExpiration;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<FlyPower>()
    };

    public void IgnoreNextInstance()
    {
        _shouldIgnoreNextInstance = true;
    }

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<FlyPower>(target, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount == base.Amount)
        {
            return;
        }

        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }

        await PowerCmd.Apply<FlyPower>(base.Owner, amount, applier, cardSource, silent: true);
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return;
        }

        decimal amountToRemove = base.Amount;
        Flash();
        _isResolvingExpiration = true;
        try
        {
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<FlyPower>(base.Owner, -amountToRemove, base.Owner, null, silent: true);
        }
        finally
        {
            _isResolvingExpiration = false;
        }
    }
}