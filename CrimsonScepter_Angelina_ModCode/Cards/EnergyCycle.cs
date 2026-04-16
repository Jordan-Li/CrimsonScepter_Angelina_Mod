using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：能量循环
/// 卡牌类型：能力牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：每回合中，打出耗能大于等于2的攻击、技能或能力牌时，回复能量，每种类型至多各触发若干次。
/// 升级后效果：变为固有。
/// </summary>
public sealed class EnergyCycle : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? new CardKeyword[] { CardKeyword.Innate }
        : System.Array.Empty<CardKeyword>();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Innate),
            HoverTipFactory.ForEnergy(this),
            HoverTipFactory.FromPower<EnergyCyclePower>()
        }
        : new IHoverTip[]
        {
            HoverTipFactory.ForEnergy(this),
            HoverTipFactory.FromPower<EnergyCyclePower>()
        };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
        new PowerVar<EnergyCyclePower>(1m)
    };

    public EnergyCycle()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<EnergyCyclePower>(
            base.Owner.Creature,
            base.DynamicVars["EnergyCyclePower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}