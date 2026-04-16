using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：DecelBinderPower
/// 效果：每当你打出攻击牌，对其目标施加迟滞。
/// </summary>
public sealed class DecelBinderPower : AngelinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StaggerPower>()
    };

    // 打出攻击牌后，对被指定的目标施加迟滞
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner || cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        if (cardPlay.Target == null || !cardPlay.Target.IsAlive)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StaggerPower>(cardPlay.Target, base.Amount, base.Owner, cardPlay.Card);
    }
}
