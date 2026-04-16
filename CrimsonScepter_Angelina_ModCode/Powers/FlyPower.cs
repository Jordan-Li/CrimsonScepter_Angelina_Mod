using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：飞行
/// 效果：
/// 1. 受到“有威力的攻击”时伤害减半
/// 2. 每次受到未被格挡的“有威力的攻击”后，失去1层
/// 3. 层数归零时移除
/// 备注：这里严格按旧版 FlyPower 的语义迁移
/// </summary>
public sealed class FlyPower : AngelinaPower
{
    private const string DamageDecreaseKey = "DamageDecrease";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(DamageDecreaseKey, 50m)
    };

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await RemoveIfDepleted();
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            await RemoveIfDepleted();
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner)
        {
            return 1m;
        }

        if (base.Amount <= 0 || !IsPoweredAttack(props))
        {
            return 1m;
        }

        return base.DynamicVars[DamageDecreaseKey].BaseValue / 100m;
    }

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

    private Task RemoveIfDepleted()
    {
        if (base.Amount <= 0)
        {
            return PowerCmd.Remove(this);
        }

        return Task.CompletedTask;
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}