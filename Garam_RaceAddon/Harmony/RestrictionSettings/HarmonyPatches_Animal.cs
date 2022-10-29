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
    [HarmonyPatch(typeof(WorkGiver_Tame))]
    [HarmonyPatch("JobOnThing")]
    public static class HarmonyPatches_WorkGiver_Tame_JobOnThing
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, Thing t, ref Job __result)
        {
            if (__result != null && !RaceAddonTools.CheckAnimal(pawn.def, t.def))
            {
                __result = null;
                JobFailReason.Is("RaceAddonRestriction_FloatMenu".Translate(), null);
            }
        }
    }
    [HarmonyPatch(typeof(WorkGiver_Train))]
    [HarmonyPatch("JobOnThing")]
    public static class HarmonyPatches_WorkGiver_Train_JobOnThing
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, Thing t, ref Job __result)
        {
            if (__result != null && !RaceAddonTools.CheckAnimal(pawn.def, t.def))
            {
                __result = null;
                JobFailReason.Is("RaceAddonRestriction_FloatMenu".Translate(), null);
            }
        }
    }
}
