using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

/// <summary>
/// 费用：0
/// 稀有度：稀有
/// 卡牌类型：攻击
/// 效果：保留。对敌方全体造成40点法术伤害。失去你最右侧的遗物。
/// 升级后效果：保留。对敌方全体造成50点法术伤害。失去你最右侧的遗物。
/// </summary>
public sealed class UltimateBigBang : AngelinaCard
{
    private static readonly Color BeamGoldBright = new(1f, 0.9f, 0.35f, 1f);
    private static readonly Color BeamGoldMid = new(1f, 0.72f, 0.16f, 1f);
    private static readonly Color BeamGoldWarm = new(1f, 0.96f, 0.75f, 1f);
    private static readonly Color ParticleGold = new(1f, 0.82f, 0.24f, 1f);

    public override bool IsSpell => true;

    // 没有遗物可失去时，这张牌不能打出。
    protected override bool IsPlayable => base.Owner?.Relics.Any() == true;

    // 可打出时高亮，提示这张牌当前满足使用条件。
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    // 只保留 Retain 关键字；法术由 IsSpell 控制。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    // 额外显示法术说明，方便理解伤害会吃法术修正。
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description"))
    ];

    // 这张牌只有一段法术伤害动态值：40，升级到 50。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(40m, ValueProp.Unpowered | ValueProp.Move),
        new CalculationBaseVar(40m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered | ValueProp.Move)
            .WithMultiplier(static (card, _) => card.Owner?.Creature?.GetPower<FocusPower>()?.Amount ?? 0m)
    ];

    public UltimateBigBang()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    // 打出时：
    // 1. 失去当前最右侧的遗物
    // 2. 播放类似超能光束的主束流与落点特效
    // 3. 对所有敌人造成法术伤害
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.Owner.Relics.Count == 0)
        {
            return;
        }

        var relicToLose = base.Owner.Relics.Last();
        await RelicCmd.Remove(relicToLose);
        await PlayHyperbeamLikeVfx();

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
    }

    // 升级后伤害从 40 提高到 50。
    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(10m);
        base.DynamicVars.CalculationBase.UpgradeValueBy(10m);
    }

    private async Task PlayHyperbeamLikeVfx()
    {
        List<Creature> enemies = (base.CombatState?.Enemies ?? Enumerable.Empty<Creature>())
            .Where(enemy => enemy.IsAlive)
            .ToList();

        if (enemies.Count == 0)
        {
            return;
        }

        Vector2 sourcePosition = GetCreatureBeamOrigin(base.Owner.Creature);
        Vector2 mainTargetPosition = GetCreatureBeamOrigin(enemies.Last());

        NHyperbeamVfx? beamVfx = NHyperbeamVfx.Create(sourcePosition, mainTargetPosition);
        if (beamVfx != null)
        {
            TintHyperbeamGold(beamVfx);
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(beamVfx);
            await Cmd.Wait(0.5f);
        }

        foreach (Creature enemy in enemies)
        {
            NHyperbeamImpactVfx? impactVfx = NHyperbeamImpactVfx.Create(sourcePosition, GetCreatureBeamOrigin(enemy));
            if (impactVfx != null)
            {
                TintImpactGold(impactVfx);
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(impactVfx);
            }
        }
    }

    private static Vector2 GetCreatureBeamOrigin(Creature creature)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        Control? hitbox = creatureNode?.Hitbox;
        if (hitbox != null)
        {
            return hitbox.GetGlobalRect().GetCenter();
        }

        return creatureNode?.VfxSpawnPosition ?? Vector2.Zero;
    }

    private static void TintHyperbeamGold(NHyperbeamVfx beamVfx)
    {
        TintCanvasItemsGold(beamVfx);

        Line2D? laserLine = beamVfx.GetNodeOrNull<Line2D>("laser/vfx_hyperbeam_laser_line");
        if (laserLine == null)
        {
            return;
        }

        Gradient goldGradient = new();
        goldGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
        goldGradient.Offsets = [0f, 0.34f, 0.68f, 1f];
        goldGradient.Colors = [BeamGoldBright, BeamGoldMid, BeamGoldBright, BeamGoldWarm];
        laserLine.Gradient = goldGradient;

        if (laserLine.Material is ShaderMaterial shaderMaterial)
        {
            ShaderMaterial beamMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
            Gradient lutGradient = new();
            lutGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
            lutGradient.Offsets = [0f, 0.33f, 0.66f, 1f];
            lutGradient.Colors = [BeamGoldBright, BeamGoldMid, BeamGoldBright, BeamGoldWarm];
            beamMaterial.SetShaderParameter("lut", new GradientTexture1D { Gradient = lutGradient });
            laserLine.Material = beamMaterial;
        }
    }

    private static void TintImpactGold(NHyperbeamImpactVfx impactVfx)
    {
        TintCanvasItemsGold(impactVfx);
    }

    private static void TintCanvasItemsGold(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is CanvasItem canvasItem && child is not Line2D)
            {
                canvasItem.SelfModulate = ParticleGold;
            }

            TintCanvasItemsGold(child);
        }
    }
}
