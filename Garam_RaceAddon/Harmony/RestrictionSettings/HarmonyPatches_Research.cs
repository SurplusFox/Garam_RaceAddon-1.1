using RimWorld;
using Verse;
using HarmonyLib;
using Verse.AI;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(WorkGiver_Researcher))]
    [HarmonyPatch("ShouldSkip")]
    public static class HarmonyPatches_WorkGiver_Researcher_ShouldSkip
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result)
            {
                ResearchProjectDef project = Find.ResearchManager.currentProj;
                if (!RaceAddonTools.CheckResearch(pawn.def, project))
                {
                    __result = true;
                }
            }
        }
    }
    [HarmonyPatch(typeof(MainTabWindow_Research))]
    [HarmonyPatch("DrawLeftRect")]
    public static class HarmonyPatches_DrawLeftRect
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var info = AccessTools.Field(typeof(SoundDefOf), "ResearchStart");
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand == info)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches_DrawLeftRect), "NoRaceWarning"));
                }
                yield return instruction;
            }
        }
        private static void NoRaceWarning(ResearchProjectDef def)
        {
            if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddonTools.CheckResearch(x.def, def)))
            {
                string text = "RaceAddonRestriction_Research".Translate(def.label);
                text += "\n\n";
                foreach (var thingDef in RaceAddonTools.AllRaceAddonThingDefs.FindAll(x => RaceAddonTools.CheckResearch(x, def)))
                {
                    text += thingDef.label + "\n";
                }
                Find.WindowStack.Add(new Dialog_MessageBox(text, null, null, null, null, null, false, null, null));
            }
        }
    }
}