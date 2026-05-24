using System;
using System.Collections.Generic;
using System.Linq;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

internal static class CreatureRingUi
{
    private sealed record RingEntry(string Key, int Order, float Progress, Color FillColor, IEnumerable<IHoverTip> HoverTips);

    private sealed class RingWidget
    {
        public required Control Hitbox { get; init; }
        public required Line2D Background { get; init; }
        public required Line2D Fill { get; init; }
        public string? Key { get; set; }
        public IEnumerable<IHoverTip> HoverTips { get; set; } = Enumerable.Empty<IHoverTip>();
    }

    private sealed class HostState
    {
        public required Creature Creature { get; init; }
        public required NHealthBar HealthBar { get; init; }
        public required Control Root { get; init; }
        public List<RingWidget> Widgets { get; } = new();
    }

    private static readonly Dictionary<Creature, HostState> HostsByCreature = new();
    private static readonly Dictionary<NHealthBar, HostState> HostsByHealthBar = new();

    private static readonly Color RingBackgroundColor = new(1f, 1f, 1f, 0.12f);
    private static readonly Color ImbalanceRingColor = new(1f, 1f, 1f, 0.92f);
    private static readonly Color WeightlessRingColor = new(1f, 0.89f, 0.26f, 0.96f);

    private const int WeightlessOrder = 0;
    private const int ImbalanceOrder = 10;
    private const float RingRadius = 10f;
    private const float RingThickness = 4f;
    private const float RingStep = 26f;
    private const float RingAnchorOffset = 16f;
    private const float RingHoverSize = 28f;

    public static void Attach(NHealthBar healthBar, Creature creature)
    {
        if (HostsByCreature.TryGetValue(creature, out HostState? oldHost))
        {
            Remove(oldHost);
        }

        Control root = new()
        {
            Name = $"CreatureRingHost_{creature.GetHashCode()}",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        healthBar.AddChild(root);

        HostState host = new()
        {
            Creature = creature,
            HealthBar = healthBar,
            Root = root
        };

        HostsByCreature[creature] = host;
        HostsByHealthBar[healthBar] = host;
        Refresh(healthBar);
    }

    public static void Refresh(Creature creature)
    {
        if (HostsByCreature.TryGetValue(creature, out HostState? host))
        {
            Refresh(host);
        }
    }

    public static void Refresh(NHealthBar healthBar)
    {
        if (HostsByHealthBar.TryGetValue(healthBar, out HostState? host))
        {
            Refresh(host);
        }
    }

    public static void RefreshLayout(NHealthBar healthBar)
    {
        if (HostsByHealthBar.TryGetValue(healthBar, out HostState? host))
        {
            Layout(host);
        }
    }

    private static void Refresh(HostState host)
    {
        if (!GodotObject.IsInstanceValid(host.Root) || !GodotObject.IsInstanceValid(host.HealthBar))
        {
            Remove(host);
            return;
        }

        List<RingEntry> entries = GetEntries(host.Creature).OrderBy(entry => entry.Order).ToList();
        EnsureWidgetCount(host, entries.Count);

        for (int i = 0; i < host.Widgets.Count; i++)
        {
            RingWidget widget = host.Widgets[i];
            bool isActive = i < entries.Count;

            widget.Hitbox.Visible = isActive;
            widget.Background.Visible = isActive;
            widget.Fill.Visible = isActive;

            if (!isActive)
            {
                widget.Key = null;
                widget.HoverTips = Enumerable.Empty<IHoverTip>();
                NHoverTipSet.Remove(widget.Hitbox);
                continue;
            }

            RingEntry entry = entries[i];
            widget.Key = entry.Key;
            widget.HoverTips = entry.HoverTips.ToList();
            widget.Fill.DefaultColor = entry.FillColor;
            widget.Fill.Points = CreateArcPoints(entry.Progress);
            widget.Background.Points = CreateArcPoints(1f);
        }

        Layout(host);
    }

    private static void Layout(HostState host)
    {
        Vector2 baseAnchor = host.HealthBar.HpBarContainer.Position + new Vector2(-RingAnchorOffset, host.HealthBar.HpBarContainer.Size.Y * 0.5f);

        for (int i = 0; i < host.Widgets.Count; i++)
        {
            RingWidget widget = host.Widgets[i];
            widget.Hitbox.Position = baseAnchor + new Vector2(-RingStep * i - RingHoverSize * 0.5f, -RingHoverSize * 0.5f);
        }
    }

    private static void EnsureWidgetCount(HostState host, int count)
    {
        while (host.Widgets.Count < count)
        {
            host.Widgets.Add(CreateWidget(host.Root));
        }
    }

    private static RingWidget CreateWidget(Control parent)
    {
        Control hitbox = new()
        {
            CustomMinimumSize = Vector2.One * RingHoverSize,
            Size = Vector2.One * RingHoverSize,
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        Line2D background = new()
        {
            Width = RingThickness,
            DefaultColor = RingBackgroundColor,
            Antialiased = true,
            Position = Vector2.One * (RingHoverSize * 0.5f)
        };

        Line2D fill = new()
        {
            Width = RingThickness,
            DefaultColor = ImbalanceRingColor,
            Antialiased = true,
            Position = Vector2.One * (RingHoverSize * 0.5f)
        };

        hitbox.AddChild(background);
        hitbox.AddChild(fill);
        parent.AddChild(hitbox);
        RingWidget widget = new RingWidget
        {
            Hitbox = hitbox,
            Background = background,
            Fill = fill
        };

        hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(() =>
        {
            if (!widget.HoverTips.Any())
            {
                return;
            }

            NHoverTipSet.Remove(hitbox);
            NHoverTipSet.CreateAndShow(hitbox, widget.HoverTips, HoverTip.GetHoverTipAlignment(hitbox));
        }));

        hitbox.Connect(Control.SignalName.MouseExited, Callable.From(() => NHoverTipSet.Remove(hitbox)));

        return widget;
    }

    private static IEnumerable<RingEntry> GetEntries(Creature creature)
    {
        WeightlessPower? weightless = creature.GetPower<WeightlessPower>();
        if (weightless != null && weightless.Amount > 0)
        {
            decimal maxStacks = 3m;
            decimal clampedAmount = Math.Clamp(weightless.Amount, 0m, maxStacks);
            yield return new RingEntry(
                "weightless",
                WeightlessOrder,
                (float)(clampedAmount / maxStacks),
                WeightlessRingColor,
                BuildWeightlessHoverTips(weightless));
        }

        ImbalancePower? imbalance = creature.GetPower<ImbalancePower>();
        if (imbalance != null && imbalance.Amount > 0)
        {
            decimal threshold = Math.Min(100m, Math.Ceiling(creature.MaxHp / 2m));
            if (threshold > 0m)
            {
                decimal clampedAmount = Math.Clamp(imbalance.Amount, 0m, threshold);
                yield return new RingEntry(
                    "imbalance",
                    ImbalanceOrder,
                    (float)(clampedAmount / threshold),
                    ImbalanceRingColor,
                    BuildImbalanceHoverTips(imbalance, threshold));
            }
        }
    }

    private static IEnumerable<IHoverTip> BuildImbalanceHoverTips(ImbalancePower power, decimal threshold)
    {
        return new IHoverTip[]
        {
            new HoverTip(power.Title, $"{power.Amount}/{threshold}"),
            HoverTipFactory.FromPower<ImbalancePower>(),
            HoverTipFactory.FromPower<WeightlessPower>()
        };
    }

    private static IEnumerable<IHoverTip> BuildWeightlessHoverTips(WeightlessPower power)
    {
        return new IHoverTip[]
        {
            new HoverTip(power.Title, $"{power.Amount}/3"),
            HoverTipFactory.FromPower<WeightlessPower>(),
            HoverTipFactory.FromPower<ImbalancePower>()
        };
    }

    private static Vector2[] CreateArcPoints(float progress)
    {
        progress = Mathf.Clamp(progress, 0f, 1f);
        if (progress <= 0f)
        {
            return Array.Empty<Vector2>();
        }

        int segmentCount = Math.Max(2, Mathf.CeilToInt(48f * progress));
        Vector2[] points = new Vector2[segmentCount + 1];
        float startAngle = Mathf.Pi;
        float endAngle = startAngle - Mathf.Tau * progress;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * RingRadius;
        }

        return points;
    }

    private static void Remove(HostState host)
    {
        HostsByCreature.Remove(host.Creature);
        HostsByHealthBar.Remove(host.HealthBar);
        if (GodotObject.IsInstanceValid(host.Root))
        {
            host.Root.QueueFree();
        }
    }
}
