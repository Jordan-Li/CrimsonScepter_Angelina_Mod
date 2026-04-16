using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;

/// <summary>
/// Power名：RealityManipulationPower
/// 效果：当你的 Exhaust 数量发生变化时，获得格挡。
/// </summary>
public sealed class RealityManipulationPower : AngelinaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    // 只要拥有者的牌进出 Exhaust，且数量发生变化，就获得格挡
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card.Owner != base.Owner.Player)
        {
            return;
        }

        PileType newPileType = card.Pile?.Type ?? PileType.None;
        bool exhaustCountChanged = oldPileType == PileType.Exhaust ^ newPileType == PileType.Exhaust;
        if (!exhaustCountChanged)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
    }
}
