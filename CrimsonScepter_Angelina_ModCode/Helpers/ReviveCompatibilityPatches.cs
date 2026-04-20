using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Helpers;

/// <summary>
/// Some monsters use custom dead/revive moves after dying instead of leaving combat.
/// If they are currently in the built-in STUNNED move, the move state machine
/// normally refuses to transition away before STUNNED is performed once.
/// That leaves revive-capable monsters in a fake-dead state with stale intents.
/// Allowing forced transitions while the owner is already dead preserves the
/// intended stun behavior while alive and still lets death handlers switch into
/// their revive/dead moves.
/// </summary>
[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.SetMoveImmediate))]
internal static class ReviveCompatibilityPatches
{
    private static void Prefix(MonsterModel __instance, MoveState state, ref bool forceTransition)
    {
        _ = state;

        if (forceTransition)
        {
            return;
        }

        if (__instance.Creature.IsDead)
        {
            forceTransition = true;
        }
    }
}