using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：二段冲刺！
/// 卡牌类型：技能牌
/// 稀有度：普通
/// 费用：1费
/// 效果：获得格挡。若本场战斗中打出过同名牌，获得临时飞行。
/// 升级后效果：提高格挡。
/// </summary>
public sealed class TwoStageSprint : AngelinaCard
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => HasBeenPlayedThisCombat;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<TemporaryFlyPower>()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(8m, ValueProp.Move),
        new PowerVar<TemporaryFlyPower>(1m)
    };

    private bool HasBeenPlayedThisCombat => GetTimesPlayedThisCombat() >= 1;

    public TwoStageSprint()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        if (GetTimesPlayedThisCombat() >= 2)
        {
            await PowerCmd.Apply<TemporaryFlyPower>(
                base.Owner.Creature,
                base.DynamicVars["TemporaryFlyPower"].BaseValue,
                base.Owner.Creature,
                this
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(4m);
    }

    private int GetTimesPlayedThisCombat()
    {
        if (base.CombatState == null)
        {
            return 0;
        }

        return CombatManager.Instance.History.CardPlaysStarted.Count(entry =>
            entry.CardPlay.Card.Owner == base.Owner &&
            entry.CardPlay.Card.Id == Id);
    }
}