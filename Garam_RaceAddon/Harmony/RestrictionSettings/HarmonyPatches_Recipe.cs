using RimWorld;
using Verse;
using HarmonyLib;
using Verse.AI;
using System.Linq;

namespace Garam_RaceAddon
{
    /*
    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch("PawnAllowedToStartAnew")]
    public static class HarmonyPatches_PawnAllowedToStartAnew
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn p, RecipeDef ___recipe, ref bool __result)
        {
            if (__result && !RaceAddonTools.CheckRecipe(p.def, ___recipe))
            {
                __result = false;
            }
        }
    }
    */
    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch("PawnAllowedToStartAnew")]
    public static class HarmonyPatches_PawnAllowedToStartAnew
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn p, RecipeDef ___recipe, ref bool __result)
        {
            if (__result && !RaceAddonTools.CheckRecipe(p.def, ___recipe))
            {
                __result = false;
                JobFailReason.Is("RaceAddonRestriction_FloatMenu".Translate(), null);
            }
        }
    }
    [HarmonyPatch(typeof(BillUtility))]
    [HarmonyPatch("MakeNewBill")]
    public static class HarmonyPatches_MakeNewBill
    {
        [HarmonyPostfix]
        private static void Postfix(RecipeDef recipe)
        {
            if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => RaceAddonTools.CheckRecipe(x.def, recipe)))
            {
                string text = "RaceAddonRestriction_Recipe".Translate(recipe.LabelCap);
                text += "\n\n";
                foreach (RaceAddonThingDef thingDef in RaceAddonTools.AllRaceAddonThingDefs.FindAll(x => RaceAddonTools.CheckRecipe(x, recipe)))
                {
                    text += thingDef.label + "\n";
                }
                Find.WindowStack.Add(new Dialog_MessageBox(text, null, null, null, null, null, false, null, null));
            }
        }
    }
}