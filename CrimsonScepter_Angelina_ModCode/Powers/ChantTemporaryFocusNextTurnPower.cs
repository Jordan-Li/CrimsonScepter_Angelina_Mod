using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：吟唱
/// 效果：下回合开始时，获得集中，然后移除自身。
/// </summary>
public sealed class ChantTemporaryFocusNextTurnPower : AngelinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<FocusPower>()
    };

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<FocusPower>(base.Owner, base.Amount, base.Owner, null);
        await PowerCmd.Remove(this);
    }
}