using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class FishingForScales : AngelinaCard
{
    private static readonly Color SplashBlue = new(0.58f, 0.84f, 1f, 1f);
    private static readonly string SlimeImpactScenePath = SceneHelper.GetScenePath("vfx/vfx_slime_impact");
    private static readonly Dictionary<string, Texture2D> RecoloredTextureCache = [];

    public override TargetType TargetType => IsUpgraded
        ? TargetType.AnyEnemy
        : TargetType.RandomEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new PowerVar<ImbalancePower>(12m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ImbalancePower>()
    ];

    public FishingForScales()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(base.CombatState, nameof(base.CombatState));

        AttackCommand attackCommand = DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay);

        if (IsUpgraded)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            attackCommand = attackCommand.Targeting(cardPlay.Target);
        }
        else
        {
            attackCommand = attackCommand.TargetingRandomOpponents(base.CombatState);
        }

        AttackCommand attackResult = await attackCommand.Execute(choiceContext);
        foreach (DamageResult result in attackResult.Results.SelectMany(results => results))
        {
            if (result.Receiver != null)
            {
                PlayBlueSlimeImpact(result.Receiver);
            }
        }

        if (attackResult.Results.SelectMany(results => results).Any(result => result.UnblockedDamage > 0))
        {
            return;
        }

        List<Creature> enemies = base.CombatState
            .GetOpponentsOf(base.Owner.Creature)
            .Where(enemy => enemy.IsAlive && enemy.IsHittable)
            .ToList();

        if (enemies.Count == 0)
        {
            return;
        }

        await PowerCmd.Apply<ImbalancePower>(choiceContext, enemies, base.DynamicVars["ImbalancePower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ImbalancePower"].UpgradeValueBy(6m);
    }

    private static void PlayBlueSlimeImpact(Creature target)
    {
        Control? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        if (vfxContainer == null)
        {
            return;
        }

        Node2D slimeImpactVfx = PreloadManager.Cache
            .GetScene(SlimeImpactScenePath)
            .Instantiate<Node2D>(PackedScene.GenEditState.Disabled);

        slimeImpactVfx.GlobalPosition = GetCreatureCenter(target);
        TintCanvasItems(slimeImpactVfx);
        vfxContainer.AddChildSafely(slimeImpactVfx);
    }

    private static Vector2 GetCreatureCenter(Creature creature)
    {
        Control? hitbox = NCombatRoom.Instance?.GetCreatureNode(creature)?.Hitbox;
        return hitbox?.GetGlobalRect().GetCenter() ?? Vector2.Zero;
    }

    private static void TintCanvasItems(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is GpuParticles2D particles)
            {
                TintParticles(particles);
            }

            if (child is CanvasItem canvasItem)
            {
                canvasItem.SelfModulate = SplashBlue;
                canvasItem.Modulate = SplashBlue;
            }

            TintCanvasItems(child);
        }
    }

    private static void TintParticles(GpuParticles2D particles)
    {
        particles.SelfModulate = SplashBlue;
        particles.Modulate = SplashBlue;
        particles.Texture = RecolorTexture(particles.Texture);

        if (particles.ProcessMaterial is not ParticleProcessMaterial processMaterial)
        {
            return;
        }

        ParticleProcessMaterial materialCopy = (ParticleProcessMaterial)processMaterial.Duplicate();
        materialCopy.Color = Recolor(materialCopy.Color);
        materialCopy.ColorRamp = RecolorGradientTexture(materialCopy.ColorRamp);
        particles.ProcessMaterial = materialCopy;
    }

    private static GradientTexture1D? RecolorGradientTexture(Texture2D? texture)
    {
        if (texture is not GradientTexture1D gradientTexture || gradientTexture.Gradient == null)
        {
            return texture as GradientTexture1D;
        }

        GradientTexture1D textureCopy = (GradientTexture1D)gradientTexture.Duplicate();
        Gradient gradientCopy = (Gradient)gradientTexture.Gradient.Duplicate();
        for (int i = 0; i < gradientCopy.GetPointCount(); i++)
        {
            gradientCopy.SetColor(i, Recolor(gradientCopy.GetColor(i)));
        }

        textureCopy.Gradient = gradientCopy;
        return textureCopy;
    }

    private static Texture2D? RecolorTexture(Texture2D? texture)
    {
        if (texture == null)
        {
            return null;
        }

        string cacheKey = texture.ResourcePath;
        if (!string.IsNullOrEmpty(cacheKey) && RecoloredTextureCache.TryGetValue(cacheKey, out Texture2D? cachedTexture))
        {
            return cachedTexture;
        }

        Image image = texture.GetImage();
        if (image == null)
        {
            return texture;
        }

        Image recoloredImage = (Image)image.Duplicate();
        int width = recoloredImage.GetWidth();
        int height = recoloredImage.GetHeight();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixel = recoloredImage.GetPixel(x, y);
                if (pixel.A <= 0.001f)
                {
                    continue;
                }

                recoloredImage.SetPixel(x, y, Recolor(pixel));
            }
        }

        ImageTexture recoloredTexture = ImageTexture.CreateFromImage(recoloredImage);
        if (!string.IsNullOrEmpty(cacheKey))
        {
            RecoloredTextureCache[cacheKey] = recoloredTexture;
        }

        return recoloredTexture;
    }

    private static Color Recolor(Color original)
    {
        float value = Math.Max(original.R, Math.Max(original.G, original.B));
        float min = Math.Min(original.R, Math.Min(original.G, original.B));
        float saturation = value <= 0.0001f
            ? 0f
            : (value - min) / value;

        float targetHue = SplashBlue.H;
        float targetSaturation = Mathf.Lerp(0.18f, 0.72f, saturation);
        float targetValue = Mathf.Lerp(value, 1f, 0.2f);

        return Color.FromHsv(targetHue, targetSaturation, targetValue, original.A);
    }
}
