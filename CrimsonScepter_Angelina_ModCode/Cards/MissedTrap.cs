using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：落空陷阱
/// 卡牌类型：攻击牌
/// 稀有度：非凡
/// 费用：0费
/// 效果：造成伤害2次。若此次伤害使敌方脱离浮空，获得能量。
/// 升级后效果：提高单段伤害与获得能量。
/// </summary>
public sealed class MissedTrap : AngelinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        new EnergyVar("EnergyGain", 1)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<FlyPower>(),
        new HoverTip(
            new LocString("powers", "AIRBORNE.title"),
            new LocString("powers", "AIRBORNE.description")),
        HoverTipFactory.ForEnergy(this)
    };

    public MissedTrap()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        bool grantedEnergy = false;

        for (int i = 0; i < 2; i++)
        {
            bool hadFly = (cardPlay.Target.GetPower<FlyPower>()?.Amount ?? 0) > 0;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            bool hasFlyNow = cardPlay.Target.IsAlive && (cardPlay.Target.GetPower<FlyPower>()?.Amount ?? 0) > 0;
            if (!grantedEnergy && hadFly && !hasFlyNow)
            {
                await PlayerCmd.GainEnergy(base.DynamicVars["EnergyGain"].BaseValue, base.Owner);
                grantedEnergy = true;
            }

            if (!cardPlay.Target.IsAlive)
            {
                break;
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1m);
        base.DynamicVars["EnergyGain"].UpgradeValueBy(1m);
    }
}