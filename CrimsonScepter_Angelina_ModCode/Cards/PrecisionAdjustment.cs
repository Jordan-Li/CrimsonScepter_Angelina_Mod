using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：精密调整
/// 卡牌类型：技能牌
/// 稀有度：普通
/// 费用：0费
/// 效果：使全体敌方目标获得失衡值。下回合其失去部分失衡值。
/// 升级后效果：提高施加的失衡值。
/// </summary>
public sealed class PrecisionAdjustment : AngelinaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ImbalancePower>(12m),
        new PowerVar<PrecisionAdjustmentPower>(5m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ImbalancePower>(),
        HoverTipFactory.FromPower<PrecisionAdjustmentPower>()
    };

    public PrecisionAdjustment()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Enumerable.Empty<Creature>())
        {
            await PowerCmd.Apply<ImbalancePower>(
                enemy,
                base.DynamicVars["ImbalancePower"].BaseValue,
                base.Owner.Creature,
                this
            );

            await PowerCmd.Apply<PrecisionAdjustmentPower>(
                enemy,
                base.DynamicVars["PrecisionAdjustmentPower"].BaseValue,
                base.Owner.Creature,
                this
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ImbalancePower"].UpgradeValueBy(3m);
    }
}