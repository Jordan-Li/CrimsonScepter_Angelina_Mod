using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：小礼物
/// 卡牌类型：攻击牌
/// 稀有度：基础
/// 费用：0费
/// 效果：造成法术伤害，并获得法术格挡
/// 升级后效果：伤害和格挡各提高1点
/// 备注：基础法术牌
/// </summary>
public sealed class LittleGift : AngelinaCard
{
    // 这张牌会提供格挡，供游戏UI和系统识别
    public override bool GainsBlock => true;

    // 额外悬浮提示：法术
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        new HoverTip(new LocString("powers", "SPELL.title"), new LocString("powers", "SPELL.description"))
    };

    // 定义两个动态变量：
    // 第一个是法术伤害，初始值为3点
    // 第二个是法术格挡，初始值为3点
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Unpowered | ValueProp.Move),
        new BlockVar(3m, ValueProp.Unpowered | ValueProp.Move)
    };

    // 费用：0费，类型：攻击牌，稀有度：基础，打击目标：任选一个敌人
    public LittleGift()
        : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    // 打出时的效果
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 如果没有目标，就直接报错，避免空引用
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 先计算当前实际法术数值
        decimal block = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Block.BaseValue);
        decimal damage = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue);

        // 先获得法术格挡
        await SpellHelper.GainBlock(base.Owner.Creature, base.Owner.Creature, block, cardPlay);

        // 再造成法术伤害
        await SpellHelper.Damage(choiceContext, base.Owner.Creature, cardPlay.Target, damage, this);
    }

    // 升级后，将伤害和格挡各提高1点
    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1m);
        base.DynamicVars.Block.UpgradeValueBy(1m);
    }

    // 给描述补充法术修正后的显示值
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        decimal displayedDamage = base.DynamicVars.Damage.BaseValue;
        decimal displayedBlock = base.DynamicVars.Block.BaseValue;

        if (base.IsMutable && base.Owner?.Creature != null)
        {
            displayedDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, displayedDamage);
            displayedBlock = SpellHelper.ModifySpellValue(base.Owner.Creature, displayedBlock);
        }

        description.Add("DisplayedDamage", displayedDamage);
        description.Add("DisplayedBlock", displayedBlock);
    }
}