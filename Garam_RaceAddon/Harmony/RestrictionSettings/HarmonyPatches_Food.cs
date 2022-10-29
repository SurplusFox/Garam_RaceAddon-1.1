using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(RaceProperties))]
    [HarmonyPatch("CanEverEat", new[] { typeof(ThingDef) })]
    public static class HarmonyPatches_CanEverEat
    {
        [HarmonyPostfix]
        private static void Postfix(ThingDef t, RaceProperties __instance, ref bool __result)
        {
            if (__result && __instance.Humanlike && DefDatabase<ThingDef>.AllDefsListForReading.First((ThingDef x) => x.race == __instance) is ThingDef thingDef)
            {
                __result = RaceAddonTools.CheckFood(thingDef, t);
            }
        }
    }
}