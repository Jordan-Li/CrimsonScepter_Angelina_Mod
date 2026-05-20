using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;

public sealed class AntiGravity : AngelinaCard
{
    private static readonly Color AntiGravityTrailColor = new(0.96f, 0.18f, 0.18f, 1f);

    public override bool IsSpell => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ImbalancePower>(),
        HoverTipFactory.FromPower<TemporaryFlyPower>(),
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description"))
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ImbalancePower>(12m),
        new DamageVar(8m, ValueProp.Unpowered | ValueProp.Move),
        new CalculationBaseVar(8m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered | ValueProp.Move)
            .WithMultiplier(static (card, _) => card.Owner?.Creature?.GetPower<FocusPower>()?.Amount ?? 0m),
        new PowerVar<TemporaryFlyPower>(1m)
    ];

    public AntiGravity()
        : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        await PlayAntiGravityStrikeVfx(base.Owner.Creature, cardPlay.Target);

        await PowerCmd.Apply<ImbalancePower>(
            cardPlay.Target,
            base.DynamicVars["ImbalancePower"].BaseValue,
            base.Owner.Creature,
            this
        );

        var damageResult = await SpellHelper.Damage(
            choiceContext,
            base.Owner.Creature,
            cardPlay.Target,
            SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue),
            this
        );

        if (damageResult?.WasTargetKilled ?? !cardPlay.Target.IsAlive)
        {
            return;
        }

        await PowerCmd.Apply<TemporaryFlyPower>(
            cardPlay.Target,
            base.DynamicVars["TemporaryFlyPower"].BaseValue,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ImbalancePower"].UpgradeValueBy(3m);
        base.DynamicVars.Damage.UpgradeValueBy(4m);
        base.DynamicVars.CalculationBase.UpgradeValueBy(4m);
    }

    private static async Task PlayAntiGravityStrikeVfx(Creature owner, Creature target)
    {
        NCreature? ownerNode = NCombatRoom.Instance?.GetCreatureNode(owner);
        NCreature? targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (ownerNode == null || targetNode == null)
        {
            return;
        }

        Vector2 ownerCenter = ownerNode.Hitbox.GetGlobalRect().GetCenter();
        Vector2 targetCenter = targetNode.Hitbox.GetGlobalRect().GetCenter();
        Vector2 sourcePosition = ownerCenter + new Vector2(0f, -120f);
        NShivThrowVfx? vfx = NShivThrowVfx.Create(sourcePosition, targetCenter, AntiGravityTrailColor);
        if (vfx == null)
        {
            return;
        }

        NCombatRoom.Instance?.CombatVfxContainer.AddChild(vfx);
        await Cmd.Wait(0.15f);
    }
}
