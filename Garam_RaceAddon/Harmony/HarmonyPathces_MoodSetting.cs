using RimWorld;
using Verse;
using System.Collections.Generic;
using HarmonyLib;

namespace Garam_RaceAddon
{    /*
    [HarmonyPatch(typeof(ThoughtHandler))]
    [HarmonyPatch("GetAllMoodThoughts")]
    public static class HarmonyPatches_GetAllMoodThoughts
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn ___pawn, List<Thought> outThoughts)
        {
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.pawnCategory.moodSetting.allMoodAllow)
                {
                    outThoughts.RemoveAll((Thought x) => thingDef.raceAddonSettings.pawnCategory.moodSetting.moodList.Contains(x.def));
                }
                else
                {
                    outThoughts.RemoveAll((Thought x) => !thingDef.raceAddonSettings.pawnCategory.moodSetting.moodList.Contains(x.def));
                }
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler))]
    [HarmonyPatch("TryGainMemory", new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class HarmonyPatches_TryGainMemory
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn ___pawn, Thought_Memory newThought)
        {
            if (RaceAddonTools.CheckMood(___pawn.def, newThought.def))
            {
                return false;
            }
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.moodSetting.replacedMoods != null)
                {
                    var replacedHediff = thingDef.raceAddonSettings.moodSetting.replacedMoods.Find(x => x.originalThoughtDef == newThought.def);
                    if (replacedHediff != null)
                    {
                        newThought.def = replacedHediff.replacedThoughtDef;
                    }
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(SituationalThoughtHandler))]
    [HarmonyPatch("TryCreateThought")]
    public static class HarmonyPatches_TryCreateThought
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn ___pawn, ThoughtDef def, ref Thought_Situational __result)
        {
            if (RaceAddonTools.CheckMood(___pawn.def, def))
            {
                __result = null;
            }
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.moodSetting.replacedMoods != null)
                {
                    var replacedHediff = thingDef.raceAddonSettings.moodSetting.replacedMoods.Find(x => x.originalThoughtDef == def);
                    if (replacedHediff != null)
                    {
                        __result.def = replacedHediff.replacedThoughtDef;
                    }
                }
            }
        }
    }
    */
    [HarmonyPatch(typeof(ThoughtUtility))]
    [HarmonyPatch("CanGetThought")]
    public static class HarmonyPatches_CanGetThought
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, ThoughtDef def, ref bool __result)
        {
            if (!RaceAddonTools.CheckMood(pawn.def, def))
            {
                __result = false;
            }
        }
    }
}