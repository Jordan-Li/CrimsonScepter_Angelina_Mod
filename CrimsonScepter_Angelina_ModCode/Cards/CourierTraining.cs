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
/// 卡牌名：信使训练
/// 卡牌类型：能力牌
/// 稀有度：非凡
/// 费用：1费
/// 效果：每回合开始时，获得1层飞行。
/// 升级后效果：变为固有。
/// </summary>
public sealed class CourierTraining : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? new CardKeyword[] { CardKeyword.Innate }
        : System.Array.Empty<CardKeyword>();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Innate),
            HoverTipFactory.FromPower<CourierTrainingPower>(),
            HoverTipFactory.FromPower<FlyPower>()
        }
        : new IHoverTip[]
        {
            HoverTipFactory.FromPower<CourierTrainingPower>(),
            HoverTipFactory.FromPower<FlyPower>()
        };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<CourierTrainingPower>(1m)
    };

    public CourierTraining()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<CourierTrainingPower>(
            base.Owner.Creature,
            base.DynamicVars["CourierTrainingPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}