using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 卡牌名：终极大爆炸
/// 卡牌类型：攻击牌
/// 稀有度：稀有
/// 费用：0费
/// 效果：失去1件遗物。对所有敌人造成大量法术伤害。本场战斗中此牌费用+1。
/// 升级后效果：提高伤害。
/// </summary>
public sealed class UltimateBigBang : AngelinaCard
{
    protected override bool IsPlayable => base.Owner?.Relics.Any() == true;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Retain
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description"))
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(40m, ValueProp.Unpowered | ValueProp.Move)
    };

    public UltimateBigBang()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 没有遗物时不可正常打出；这里再做一次保护
        if (base.Owner.Relics.Count == 0)
        {
            return;
        }

        // 失去最后一个遗物
        var relicToLose = base.Owner.Relics.Last();
        await RelicCmd.Remove(relicToLose);

        // 对所有敌人造成法术伤害
        decimal damage = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue);
        List<Creature> enemies = (base.CombatState?.HittableEnemies ?? Enumerable.Empty<Creature>())
            .Where(enemy => enemy.IsAlive)
            .ToList();

        foreach (Creature enemy in enemies)
        {
            await SpellHelper.Damage(
                choiceContext,
                base.Owner.Creature,
                enemy,
                damage,
                this
            );
        }

        // 本场战斗中此牌费用 +1
        base.EnergyCost.AddThisCombat(1);
        base.InvokeEnergyCostChanged();
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(10m);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        decimal displayedDamage = base.DynamicVars.Damage.BaseValue;
        if (base.IsMutable && base.Owner?.Creature != null)
        {
            displayedDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, displayedDamage);
        }

        description.Add("DisplayedDamage", displayedDamage);
    }
}