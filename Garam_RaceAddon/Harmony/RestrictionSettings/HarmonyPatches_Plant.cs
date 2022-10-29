using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using System;
using Verse.AI;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(WorkGiver_GrowerSow))]
    [HarmonyPatch("ExtraRequirements")]
    public static class HarmonyPatches_ExtraRequirements
    {
        [HarmonyPostfix]
        private static void Postfix(IPlantToGrowSettable settable, Pawn pawn, ref bool __result)
        {
            if (__result)
            {
                ThingDef plant = WorkGiver_Grower.CalculateWantedPlantDef((settable as Zone_Growing)?.Cells[0] ?? ((Thing)settable).Position, pawn.Map);
                __result = RaceAddonTools.CheckPlant(pawn.def, plant);
            }
        }
    }
    [HarmonyPatch(typeof(WorkGiver_GrowerHarvest))]
    [HarmonyPatch("HasJobOnCell")]
    public static class HarmonyPatches_HasJobOnCell
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if (__result)
            {
                ThingDef plant = c.GetPlant(map: pawn.Map).def;
                __result = RaceAddonTools.CheckPlant(pawn.def, plant);
            }
        }
    }
    [HarmonyPatch(typeof(Command_SetPlantToGrow))]
    [HarmonyPatch("ProcessInput")]
    public static class HarmonyPatches_ProcessInput
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var info = AccessTools.Method(typeof(List<FloatMenuOption>), "Add");
            var type = AccessTools.FirstInner(typeof(Command_SetPlantToGrow), x => x.Name.Contains("5_0"));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand == info)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type, "plantDef"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches_ProcessInput), "Check"));
                }
                yield return instruction;
            }
        }
        private static FloatMenuOption Check(FloatMenuOption menu, ThingDef plantDef)
        {
            if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddonTools.CheckPlant(x.def, plantDef)))
            {
                menu.Label = plantDef.LabelCap + " (" + "RaceAddonRestriction_FloatMenu".Translate() + ")";
                menu.Disabled = true;
            }
            return menu;
        }
    }
}