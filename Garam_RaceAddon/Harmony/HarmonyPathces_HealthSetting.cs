using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(Pawn_AgeTracker))]
    [HarmonyPatch("BirthdayBiological")]
    public static class Patch_Pawn_AgeTracker_BirthdayBiological
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn ___pawn)
        {
            if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.antiAging)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(AgeInjuryUtility))]
    [HarmonyPatch("GenerateRandomOldAgeInjuries")]
    public static class Patch_AgeInjuryUtility_GenerateRandomOldAgeInjuries
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.antiAging)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(AgeInjuryUtility))]
    [HarmonyPatch("RandomHediffsToGainOnBirthday", new[] { typeof(ThingDef), typeof(int) })]
    public static class Patch_AgeInjuryUtility_RandomHediffsToGainOnBirthday
    {
        [HarmonyPrefix]
        private static bool Prefix(ThingDef raceDef)
        {
            if (raceDef is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.antiAging)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Hediff_Injury))]
    [HarmonyPatch("Heal")]
    public static class Patch_Hediff_Injury_Heal
    {
        [HarmonyPrefix]
        private static bool Prefix(Hediff_Injury __instance, ref float amount)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                amount *= thingDef.raceAddonSettings.healthSetting.healingFactor;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(HediffSet))]
    [HarmonyPatch("CalculatePain")]
    public static class Patch_HediffSet_CalculatePain
    {
        [HarmonyPostfix]
        private static void Postfix(HediffSet __instance, ref float __result)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                __result *= thingDef.raceAddonSettings.healthSetting.painFactor;
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("DropBloodFilth")]
    public static class Patch_Pawn_HealthTracker_DropBloodFilth
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn ___pawn)
        {
            if (___pawn.def is RaceAddonThingDef thingDef && !thingDef.raceAddonSettings.healthSetting.dropBloodFilth)
            {
                return false;
            }
            return true;
        }
    }
}