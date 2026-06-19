using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Abstracts;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Powers;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
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

/// <summary>
/// 费用：2
/// 稀有度：罕见
/// 卡牌类型：攻击
/// 效果：对所有敌方造成20点法术伤害。如果有目标被斩杀，则这场战斗不再有卡牌奖励。
/// 升级后效果：对所有敌方造成26点法术伤害。如果有目标被斩杀，则这场战斗不再有卡牌奖励。
/// </summary>
public sealed class AnnihilationParticle : AngelinaCard
{
    private static readonly HashSet<(CombatRoom Room, ulong PlayerNetId)> PendingNoCardRewards = [];
    private static readonly Color SweepRed = new(1f, 0.22f, 0.16f, 1f);

    // 这张牌会用到斩杀、法术伤害以及“本场战斗无卡牌奖励”的悬浮说明。
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        new HoverTip(
            new LocString("powers", "SPELL.title"),
            new LocString("powers", "SPELL.description")),
        HoverTipFactory.FromPower<AnnihilationParticleNoCardRewardPower>()
    ];

    // 维护法术伤害动态值。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Unpowered | ValueProp.Move),
        new CalculationBaseVar(20m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered | ValueProp.Move)
            .WithMultiplier(static (card, _) => card.Owner?.Creature?.GetPower<FocusPower>()?.Amount ?? 0m)
    ];

    // 这是攻击牌，但伤害部分按法术伤害结算。
    public override bool IsSpell => true;

    public AnnihilationParticle()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        CombatRoom? combatRoom = base.CombatState?.RunState?.CurrentRoom as CombatRoom;
        if (base.CombatState == null || combatRoom == null)
        {
            return;
        }

        HashSet<Creature> fatalEligibleTargets = base.CombatState.HittableEnemies
            .Where(enemy => enemy.IsAlive && enemy.Powers.All(power => power.ShouldOwnerDeathTriggerFatal()))
            .ToHashSet();

        List<Creature> hittableEnemies = base.CombatState.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        await PlaySweepingBeamLikeVfx(hittableEnemies);

        decimal spellDamage = SpellHelper.ModifySpellValue(base.Owner.Creature, base.DynamicVars.Damage.BaseValue);
        IEnumerable<DamageResult> damageResults = await SpellHelper.DamageAll(
            choiceContext,
            base.Owner.Creature,
            hittableEnemies,
            spellDamage,
            this);

        bool triggeredFatal = damageResults.Any(result =>
            result.WasTargetKilled &&
            fatalEligibleTargets.Contains(result.Receiver));

        if (!triggeredFatal)
        {
            return;
        }

        PendingNoCardRewards.Add((combatRoom, base.Owner.NetId));

        if (!base.Owner.Creature.HasPower<AnnihilationParticleNoCardRewardPower>())
        {
            await PowerCmd.Apply<AnnihilationParticleNoCardRewardPower>(
                choiceContext,
                base.Owner.Creature,
                1m,
                base.Owner.Creature,
                this
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(6m);
        base.DynamicVars.CalculationBase.UpgradeValueBy(6m);
    }

    // 奖励结算补丁会在战斗奖励生成时消费这条“移除卡牌奖励”记录。
    internal static bool ConsumePendingNoCardReward(CombatRoom room, ulong playerNetId)
    {
        return PendingNoCardRewards.Remove((room, playerNetId));
    }

    private async Task PlaySweepingBeamLikeVfx(List<Creature> targets)
    {
        if (targets.Count == 0)
        {
            return;
        }

        Array<Vector2> targetPositions = [];
        foreach (Creature target in targets)
        {
            targetPositions.Add(GetCreatureCenter(target));
        }

        NSweepingBeamVfx? sweepingBeamVfx = NSweepingBeamVfx.Create(GetCreatureCenter(base.Owner.Creature), targetPositions);
        if (sweepingBeamVfx == null)
        {
            return;
        }

        TintCanvasItemsRed(sweepingBeamVfx);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(sweepingBeamVfx);
        await Cmd.Wait(0.5f);
    }

    private static Vector2 GetCreatureCenter(Creature creature)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        Control? hitbox = creatureNode?.Hitbox;
        if (hitbox != null)
        {
            return hitbox.GetGlobalRect().GetCenter();
        }

        return creatureNode?.VfxSpawnPosition ?? Vector2.Zero;
    }

    private static void TintCanvasItemsRed(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is CanvasItem canvasItem)
            {
                canvasItem.SelfModulate = SweepRed;
            }

            TintCanvasItemsRed(child);
        }
    }
}
