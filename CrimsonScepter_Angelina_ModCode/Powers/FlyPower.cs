using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：飞行
/// Power类型：状态型Power
/// 效果：
/// 1. 让有威力的攻击伤害减半
/// 2. 每次吃到未被格挡的有威力攻击后失去1层
/// 3. 层数归零时自动移除
/// 备注：这是角色“飞行/浮空”体系的基础状态
/// </summary>
public sealed class FlyPower : AngelinaPower
{
    private const string DamageDecreaseKey = "DamageDecrease";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    /// <summary>
    /// DamageDecrease 用来把减伤比例写进说明，目前固定为 50%。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(DamageDecreaseKey, 50m)];

    /// <summary>
    /// 初次施加后若层数已归零，直接移除自身。
    /// 玩家持有飞行时，还需要主动刷新敌方意图预览。
    /// </summary>
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await RemoveIfDepleted();
        await RefreshEnemyIntentsIfPlayerOwned(base.Owner);
    }

    /// <summary>
    /// 自己的层数发生变化后，同样检查是否需要清空。
    /// 玩家持有飞行时，还需要主动刷新敌方意图预览。
    /// </summary>
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this)
        {
            return;
        }

        await RemoveIfDepleted();
        await RefreshEnemyIntentsIfPlayerOwned(base.Owner);
    }

    public override Task AfterRemoved(Creature oldOwner) => RefreshEnemyIntentsIfPlayerOwned(oldOwner);

    /// <summary>
    /// 只有受到有威力的攻击时才把伤害压到 50%。
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return target == base.Owner && base.Amount > 0 && IsPoweredAttack(props)
            ? base.DynamicVars[DamageDecreaseKey].BaseValue / 100m
            : 1m;
    }

    /// <summary>
    /// 真正吃到未被格挡的有威力攻击后失去 1 层飞行。
    /// </summary>
    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner || base.Amount <= 0 || result.UnblockedDamage == 0 || !IsPoweredAttack(props))
        {
            return Task.CompletedTask;
        }

        return PowerCmd.Decrement(this);
    }

    /// <summary>
    /// 飞行层数耗尽时移除自身。
    /// </summary>
    private Task RemoveIfDepleted()
    {
        if (base.Amount <= 0)
        {
            return PowerCmd.Remove(this);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 这里把“有威力的攻击”定义成 Move 且不是 Unpowered。
    /// </summary>
    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }

    private static async Task RefreshEnemyIntentsIfPlayerOwned(Creature owner)
    {
        if (!CombatManager.Instance.IsInProgress || !owner.IsPlayer)
        {
            return;
        }

        CombatState? combatState = owner.CombatState;
        if (combatState == null)
        {
            return;
        }

        foreach (Creature enemy in combatState.Enemies)
        {
            if (!enemy.IsMonster || !enemy.IsAlive)
            {
                continue;
            }

            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(enemy);
            if (creatureNode != null)
            {
                await creatureNode.RefreshIntents();
            }
        }
    }
}
