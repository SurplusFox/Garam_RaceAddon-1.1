using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(WildManUtility))]
    [HarmonyPatch("IsWildMan")]
    public static class HarmonyPatches_IsWildMan
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result && p.kindDef is RaceAddonPawnKindDef pkd && RaceAddonTools.IsWildMan(pkd))
            {
                __result = true;
            }
        }
    }
    [HarmonyPatch(typeof(TraderCaravanUtility))]
    [HarmonyPatch("GetTraderCaravanRole")]
    public static class HarmonyPatches_GetTraderCaravanRole
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn p, ref TraderCaravanRole __result)
        {
            if (__result == TraderCaravanRole.Guard && p.kindDef is RaceAddonPawnKindDef pkd && RaceAddonTools.IsSlave(pkd))
            {
                __result = TraderCaravanRole.Chattel;
            }
        }
    }
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("ChangeKind")]
    public static class HarmonyPatches_ChangeKind
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn __instance, PawnKindDef newKindDef)
        {
            if (DefDatabase<RaceAddonPawnKindDef>.AllDefsListForReading.Find(x => x.pawnKindDefReplacement.Any(y => y.originalPawnKindDef == newKindDef)) is var pkd && pkd != null)
            {
                __instance.kindDef = pkd;
            }
            return;
        }
    }
    [HarmonyPatch(typeof(PawnBioAndNameGenerator))]
    [HarmonyPatch("GetBackstoryCategoryFiltersFor")]
    public static class HarmonyPatches_GetBackstoryCategoryFiltersFor
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, FactionDef faction, ref List<BackstoryCategoryFilter> __result)
        {
            if (pawn.kindDef is RaceAddonPawnKindDef kindDef && kindDef.onlyUseThisBackstoryCategoryes)
            {
                __result = kindDef.backstoryFilters;
            }
        }
    }
    [HarmonyPatch(typeof(ThinkNode_ConditionalPawnKind))]
    [HarmonyPatch("Satisfied")]
    public static class HarmonyPatches_DeepCopy
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, PawnKindDef ___pawnKind, ref bool __result)
        {
            if (__result == false && ___pawnKind == PawnKindDefOf.WildMan && pawn.kindDef is RaceAddonPawnKindDef pkd && RaceAddonTools.IsWildMan(pkd))
            {
                __result = true;
            }
        }
    }
}
