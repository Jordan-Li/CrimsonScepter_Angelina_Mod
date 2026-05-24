using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Timer = Godot.Timer;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

public static class DeliveryOrbitVfx
{
    private sealed class DotSlot
    {
        public required Polygon2D OuterGlow { get; init; }
        public required Polygon2D MidGlow { get; init; }
        public required Polygon2D InnerGlow { get; init; }
        public float Presence { get; set; }
        public float TargetPresence { get; set; }
    }

    private sealed class OrbitState
    {
        public required Creature Owner { get; init; }
        public required Node2D FrontRoot { get; init; }
        public required Timer Timer { get; init; }
        public List<DotSlot> Slots { get; } = new();
        public int TargetDotCount { get; set; }
        public float OrbitTime { get; set; }
        public bool IsFadingOutAll { get; set; }
    }

    private static readonly Dictionary<Creature, OrbitState> ActiveByCreature = new();
    private static readonly Vector2[] OuterGlowPolygon = CreateCirclePolygon(24, 11.2f);
    private static readonly Vector2[] MidGlowPolygon = CreateCirclePolygon(20, 7.4f);
    private static readonly Vector2[] InnerGlowPolygon = CreateCirclePolygon(18, 6.4f);
    private static readonly Color OuterGlowColor = new(1f, 0.16f, 0.16f, 0.10f);
    private static readonly Color MidGlowColor = new(1f, 0.24f, 0.24f, 0.20f);
    private static readonly Color InnerGlowColor = new(1f, 0.42f, 0.42f, 0.74f);
    private const float FrontFadeBand = 0.24f;
    private const float PresenceLerpSpeed = 4.6f;
    private const float FadeOutAllSpeed = 2.9f;
    private const float RemovalThreshold = 0.02f;

    public static void Sync(Creature owner, int dotCount)
    {
        if (dotCount <= 0)
        {
            FadeOutAll(owner);
            return;
        }

        if (!ActiveByCreature.TryGetValue(owner, out OrbitState? state) ||
            !GodotObject.IsInstanceValid(state.FrontRoot))
        {
            state = CreateState(owner);
            ActiveByCreature[owner] = state;
        }

        state.IsFadingOutAll = false;
        state.TargetDotCount = dotCount;
        EnsureSlotCapacity(state, state.TargetDotCount);

        for (int i = 0; i < state.Slots.Count; i++)
        {
            state.Slots[i].TargetPresence = i < state.TargetDotCount ? 1f : 0f;
        }

        UpdateState(state, 0f);
    }

    public static void FadeOutAll(Creature owner)
    {
        if (!ActiveByCreature.TryGetValue(owner, out OrbitState? state) ||
            !GodotObject.IsInstanceValid(state.FrontRoot))
        {
            return;
        }

        state.IsFadingOutAll = true;
        state.TargetDotCount = 0;
        foreach (DotSlot slot in state.Slots)
        {
            slot.TargetPresence = 0f;
        }

        UpdateState(state, 0f);
    }

    public static void Remove(Creature owner)
    {
        if (!ActiveByCreature.TryGetValue(owner, out OrbitState? state))
        {
            return;
        }

        ActiveByCreature.Remove(owner);
        if (GodotObject.IsInstanceValid(state.Timer))
        {
            state.Timer.QueueFree();
        }

        if (GodotObject.IsInstanceValid(state.FrontRoot))
        {
            state.FrontRoot.QueueFree();
        }
    }

    private static OrbitState CreateState(Creature owner)
    {
        Node2D frontRoot = new()
        {
            Name = $"DeliveryOrbitFront_{owner.GetHashCode()}",
            TopLevel = true,
            ZIndex = 13
        };

        Timer timer = new()
        {
            WaitTime = 0.016,
            OneShot = false,
            Autostart = true,
            ProcessCallback = Timer.TimerProcessCallback.Idle
        };

        frontRoot.AddChild(timer);

        OrbitState state = new()
        {
            Owner = owner,
            FrontRoot = frontRoot,
            Timer = timer
        };

        timer.Timeout += () => UpdateState(state, (float)timer.WaitTime);

        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(frontRoot);
        return state;
    }

    private static void EnsureSlotCapacity(OrbitState state, int dotCount)
    {
        while (state.Slots.Count < dotCount)
        {
            Polygon2D outerGlow = new()
            {
                Polygon = OuterGlowPolygon,
                Color = WithAlpha(OuterGlowColor, 0f)
            };

            Polygon2D midGlow = new()
            {
                Polygon = MidGlowPolygon,
                Color = WithAlpha(MidGlowColor, 0f)
            };

            Polygon2D innerGlow = new()
            {
                Polygon = InnerGlowPolygon,
                Color = WithAlpha(InnerGlowColor, 0f)
            };

            state.FrontRoot.AddChild(outerGlow);
            state.FrontRoot.AddChild(midGlow);
            state.FrontRoot.AddChild(innerGlow);
            state.Slots.Add(new DotSlot
            {
                OuterGlow = outerGlow,
                MidGlow = midGlow,
                InnerGlow = innerGlow,
                Presence = 0f,
                TargetPresence = 1f
            });
        }
    }

    private static void UpdateState(OrbitState state, float delta)
    {
        NCreature? ownerNode = NCombatRoom.Instance?.GetCreatureNode(state.Owner);
        if (ownerNode == null)
        {
            Remove(state.Owner);
            return;
        }

        state.OrbitTime += delta;

        Vector2 center = ownerNode.Hitbox.GetGlobalRect().GetCenter() + new Vector2(0f, -28f);
        state.FrontRoot.GlobalPosition = center;

        float radiusX = 66f;
        float radiusY = 40f;
        float baseAngle = state.OrbitTime * Mathf.Tau * 0.22f;
        int slotCount = state.Slots.Count;

        for (int i = 0; i < slotCount; i++)
        {
            DotSlot slot = state.Slots[i];
            float lerpSpeed = state.IsFadingOutAll ? FadeOutAllSpeed : PresenceLerpSpeed;
            slot.Presence = Mathf.MoveToward(slot.Presence, slot.TargetPresence, delta * lerpSpeed);

            float progress = slotCount == 1 ? 0f : (float)i / slotCount;
            float angle = baseAngle + progress * Mathf.Tau;
            Vector2 offset = new(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);

            slot.OuterGlow.Position = offset;
            slot.MidGlow.Position = offset;
            slot.InnerGlow.Position = offset;

            float frontness = Mathf.Clamp(Mathf.Sin(angle) / FrontFadeBand, 0f, 1f);
            float frontAlpha = frontness * frontness * (3f - 2f * frontness);
            float totalAlpha = frontAlpha * slot.Presence;

            slot.OuterGlow.Visible = totalAlpha > 0.001f;
            slot.MidGlow.Visible = totalAlpha > 0.001f;
            slot.InnerGlow.Visible = totalAlpha > 0.001f;
            slot.OuterGlow.Color = WithAlpha(OuterGlowColor, OuterGlowColor.A * totalAlpha);
            slot.MidGlow.Color = WithAlpha(MidGlowColor, MidGlowColor.A * totalAlpha);
            slot.InnerGlow.Color = WithAlpha(InnerGlowColor, InnerGlowColor.A * totalAlpha);
            slot.OuterGlow.Scale = Vector2.One * (0.96f + 0.10f * totalAlpha);
            slot.MidGlow.Scale = Vector2.One * (0.98f + 0.10f * totalAlpha);
            slot.InnerGlow.Scale = Vector2.One * (1.00f + 0.14f * totalAlpha);
        }

        TrimFadedSlots(state);

        if (state.IsFadingOutAll && state.Slots.Count == 0)
        {
            Remove(state.Owner);
        }
    }

    private static void TrimFadedSlots(OrbitState state)
    {
        for (int i = state.Slots.Count - 1; i >= 0; i--)
        {
            DotSlot slot = state.Slots[i];
            if (!state.IsFadingOutAll)
            {
                break;
            }

            if (slot.Presence > RemovalThreshold)
            {
                break;
            }

            if (GodotObject.IsInstanceValid(slot.OuterGlow))
            {
                slot.OuterGlow.QueueFree();
            }

            if (GodotObject.IsInstanceValid(slot.MidGlow))
            {
                slot.MidGlow.QueueFree();
            }

            if (GodotObject.IsInstanceValid(slot.InnerGlow))
            {
                slot.InnerGlow.QueueFree();
            }

            state.Slots.RemoveAt(i);
        }
    }

    private static Vector2[] CreateCirclePolygon(int pointCount, float radius)
    {
        Vector2[] points = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float angle = Mathf.Tau * i / pointCount;
            points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        return points;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }
}
