using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;
using System.Text;

namespace Garam_RaceAddon
{
    /*
    [HarmonyPatch(typeof(GameRules))]
    [HarmonyPatch("DesignatorAllowed")]
    public static class HarmonyPatches_DesignatorAllowed
    {
        [HarmonyPostfix]
        private static void Postfix(Designator d, ref bool __result)
        {
            if (__result && d is Designator_Build target && target.PlacingDef is ThingDef def)
            {
                __result = PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddonTools.CheckBuilding(x.def, def));
            }
        }
    }
    [HarmonyPatch(typeof(GenConstruct))]
    [HarmonyPatch("CanConstruct")]
    public static class HarmonyPatches_CanConstruct
    {
        [HarmonyPostfix]
        private static void Postfix(Thing t, Pawn p, ref bool __result)
        {
            if (__result && !RaceAddonTools.CheckBuilding(p.def, t.def))
            {
                __result = false;
            }
        }
    }
    */
    [StaticConstructorOnStartup]
    public static class BuildingDescriptionEditor
    {
        static BuildingDescriptionEditor()
        {
            foreach (var def in RaceAddonTools.BuildingRestrictions)
            {
                StringBuilder stringBuilder = new StringBuilder("\n\n");
                stringBuilder.AppendLine("RaceAddonRestriction_BuildingDescriptionEditor".Translate());
                foreach (var thingDef in RaceAddonTools.AllRaceAddonThingDefs)
                {
                    stringBuilder.AppendLine(thingDef.label);
                }
                def.description += stringBuilder.ToString();
            }
        }
    }
    [HarmonyPatch(typeof(GameRules))]
    [HarmonyPatch("DesignatorAllowed")]
    public static class HarmonyPatches_DesignatorAllowed
    {
        [HarmonyPostfix]
        private static void Postfix(ref Designator d, ref bool __result)
        {
            if (__result && d is Designator_Build target && target.PlacingDef is ThingDef def && RaceAddonTools.BuildingRestrictions.Contains(def))
            {
                if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddonTools.CheckBuilding(x.def, def)))
                {
                    d.disabled = true;
                    d.disabledReason = "RaceAddonRestriction_Designator".Translate();
                }
            }
        }
    }
    [HarmonyPatch(typeof(BuildCopyCommandUtility))]
    [HarmonyPatch("BuildCopyCommand")]
    public static class HarmonyPatches_BuildCopyCommand
    {
        [HarmonyPostfix]
        private static void Postfix(BuildableDef buildable, ref Command __result)
        {
            if (__result != null && buildable is ThingDef def)
            {
                if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddonTools.CheckBuilding(x.def, def)))
                {
                    __result.disabled = true;
                    __result.disabledReason = "RaceAddonRestriction_Designator".Translate();
                }
            }
        }
    }
    [HarmonyPatch(typeof(GenConstruct))]
    [HarmonyPatch("CanConstruct")]
    public static class HarmonyPatches_CanConstruct
    {
        [HarmonyPostfix]
        private static void Postfix(Thing t, Pawn p, ref bool __result)
        {
            if (__result && t.def.entityDefToBuild is ThingDef def)
            {
                if (!RaceAddonTools.CheckBuilding(p.def, def))
                {
                    __result = false;
                    JobFailReason.Is("RaceAddonRestriction_FloatMenu".Translate(), null);
                }
            }
        }
    }
}