using System;
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
/// 卡牌名：微粒模式
/// 卡牌类型：能力牌
/// 稀有度：稀有
/// 费用：3费
/// 效果：你打出的攻击牌造成的伤害减半。每张攻击牌每回合首次打出时，改为返回手牌，并在本回合可以免费打出一次。
/// 升级后效果：移除消逝。
/// </summary>
public sealed class ParticleMode : AngelinaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded
        ? Array.Empty<CardKeyword>()
        : new CardKeyword[] { CardKeyword.Ethereal };

    // 提示：未升级时显示消逝；始终显示对应 Power
    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
        {
            HoverTipFactory.FromPower<ParticleModePower>()
        }
        : new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
            HoverTipFactory.FromPower<ParticleModePower>()
        };

    // 定义 Power 层数变量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ParticleModePower>(1m)
    };

    // 费用：3费，类型：能力牌，稀有度：稀有，自身目标
    public ParticleMode()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    // 打出时：施加微粒模式 Power
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ParticleModePower>(
            base.Owner.Creature,
            base.DynamicVars["ParticleModePower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    // 升级后移除消逝
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}