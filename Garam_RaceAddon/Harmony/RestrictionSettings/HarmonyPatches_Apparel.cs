using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using Verse.AI;
using RimWorld.Planet;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnApparelGenerator))]
    [HarmonyPatch("GenerateStartingApparelFor")]
    public static class HarmonyPatches_GenerateStartingApparelFor
    {
        private static List<ThingStuffPair> savedAllApparelPairs;
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, ref List<ThingStuffPair> ___allApparelPairs)
        {
            if (savedAllApparelPairs == null)
            {
                savedAllApparelPairs = ___allApparelPairs.ListFullCopy();
            }
            ___allApparelPairs = savedAllApparelPairs.FindAll((ThingStuffPair x) => RaceAddonTools.CheckApparel(pawn, x.thing));
            return true;
        }
    }
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel))]
    [HarmonyPatch("ApparelScoreGain")]
    public static class HarmonyPatches_ApparelScoreGain
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (!RaceAddonTools.CheckApparel(pawn, ap.def))
            {
                __result = -100f;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(JobGiver_PrisonerGetDressed))]
    [HarmonyPatch("FindGarmentCoveringPart")]
    public static class HarmonyPatches_FindGarmentCoveringPart
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, BodyPartGroupDef bodyPartGroupDef, ref Apparel __result)
        {
            Room room = pawn.GetRoom(RegionType.Set_Passable);
            if (room.isPrisonCell)
            {
                foreach (IntVec3 current in room.Cells)
                {
                    List<Thing> thingList = current.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i] is Apparel apparel &&
                            RaceAddonTools.CheckApparel(pawn, apparel.def) &&
                            apparel.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) &&
                            pawn.CanReserve(apparel, 1, -1, null, false) && !apparel.IsBurning() &&
                            ApparelUtility.HasPartsToWear(pawn, apparel.def))
                        {
                            __result = apparel;
                            return false;
                        }
                    }
                }
            }
            __result = null;
            return false;
        }
    }
}
