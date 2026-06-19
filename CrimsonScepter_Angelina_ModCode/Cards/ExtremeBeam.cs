using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class ExtremeBeam : DeliveredCardModel
{
    private const int BaseDamage = 5;

    private const int BaseCost = 0;

    private static readonly Color BeamRedBright = new(1f, 0.36f, 0.36f, 1f);
    private static readonly Color BeamRedCore = new(1f, 0.08f, 0.08f, 1f);
    private static readonly Color BeamRedDark = new(0.65f, 0.02f, 0.02f, 1f);
    private static readonly Color BeamRedParticles = new(1f, 0.18f, 0.15f, 1f);

    private int currentDamage = BaseDamage;

    public override bool IsSpell => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => WithDeliveredTip(
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description")),
        HoverTipFactory.ForEnergy(this));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Unpowered | ValueProp.Move),
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered | ValueProp.Move)
            .WithMultiplier(static (card, _) => card.Owner?.Creature?.GetPower<FocusPower>()?.Amount ?? 0m),
        new EnergyVar(1)
    ];

    public ExtremeBeam()
        : base(BaseCost, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (base.CombatState == null)
        {
            return;
        }

        decimal spellDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        await PlayExtremeBeamVfx(spellDamage);
        await SpellHelper.DamageAll(
            choiceContext,
            base.Owner.Creature,
            base.CombatState.HittableEnemies,
            spellDamage,
            this);

        currentDamage *= 2;
        RefreshDisplayedState();

        base.EnergyCost.AddThisCombat(base.DynamicVars.Energy.IntValue);
        base.InvokeEnergyCostChanged();
        NCard.FindOnTable(this)?.PlayRandomizeCostAnim();
    }

    protected override Task OnDelivered(DeliveryPower deliveryPower)
    {
        _ = deliveryPower;
        ResetCombatState();
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStartLate()
    {
        ResetCombatState();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _ = room;
        ResetCombatState();
        return Task.CompletedTask;
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileType = base.GetResultPileTypeForCardPlay();
        if (!IsUpgraded || resultPileType != PileType.Discard)
        {
            return resultPileType;
        }

        return PileType.Hand;
    }

    protected override void OnUpgrade()
    {
    }

    private void ResetCombatState()
    {
        currentDamage = BaseDamage;
        base.EnergyCost.SetThisCombat(BaseCost);
        RefreshDisplayedState();
        base.InvokeEnergyCostChanged();
    }

    private void RefreshDisplayedState()
    {
        base.DynamicVars.Damage.BaseValue = currentDamage;
        base.DynamicVars.CalculationBase.BaseValue = currentDamage;
    }

    private async Task PlayExtremeBeamVfx(decimal spellDamage)
    {
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        if (vfxContainer == null)
        {
            return;
        }

        Vector2 sourcePosition = GetCreatureCenter(base.Owner.Creature);
        NHyperbeamVfx? beamRoot = NHyperbeamVfx.Create(sourcePosition, sourcePosition + Vector2.Right * 1800f);
        if (beamRoot == null)
        {
            return;
        }

        Node2D? anticipationNode = beamRoot.GetNodeOrNull<Node2D>("anticipation");
        Line2D? laserLine = beamRoot.GetNodeOrNull<Line2D>("laser/vfx_hyperbeam_laser_line");

        if (anticipationNode != null)
        {
            anticipationNode.Visible = false;
        }

        if (laserLine == null)
        {
            beamRoot.QueueFree();
            return;
        }

        ApplyBeamLineStyle(laserLine, spellDamage);
        TintCanvasItems(beamRoot, BeamRedParticles);

        vfxContainer.AddChildSafely(beamRoot);
        await Cmd.Wait(0.22f);
    }

    private static Vector2 GetCreatureCenter(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        Control? hitbox = creatureNode?.Hitbox;
        if (hitbox != null)
        {
            return hitbox.GetGlobalRect().GetCenter();
        }

        return creatureNode?.VfxSpawnPosition ?? Vector2.Zero;
    }

    private static void ApplyBeamLineStyle(Line2D laserLine, decimal spellDamage)
    {
        float damageScale = Mathf.Clamp((float)spellDamage, BaseDamage, 80f);
        laserLine.Width = Mathf.Clamp(180f + (damageScale - BaseDamage) * 12f, 180f, 1080f);

        Gradient redGradient = new();
        redGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
        redGradient.Offsets = [0f, 0.25f, 0.5f, 0.75f, 1f];
        redGradient.Colors = [BeamRedDark, BeamRedBright, BeamRedCore, BeamRedBright, BeamRedDark];
        laserLine.Gradient = redGradient;

        if (laserLine.Material is ShaderMaterial shaderMaterial)
        {
            ShaderMaterial beamMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
            Gradient lutGradient = new();
            lutGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
            lutGradient.Offsets = [0f, 0.33f, 0.66f, 1f];
            lutGradient.Colors = [BeamRedDark, BeamRedBright, BeamRedCore, BeamRedBright];
            beamMaterial.SetShaderParameter("lut", new GradientTexture1D { Gradient = lutGradient });
            laserLine.Material = beamMaterial;
        }
    }
    private static void TintCanvasItems(Node root, Color tint)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is CanvasItem canvasItem && child is not Line2D)
            {
                canvasItem.SelfModulate = tint;
            }

            TintCanvasItems(child, tint);
        }
    }
}
