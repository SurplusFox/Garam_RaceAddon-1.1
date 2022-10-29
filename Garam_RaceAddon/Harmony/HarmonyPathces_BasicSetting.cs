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
    [HarmonyPatch(typeof(TraitSet))]
    [HarmonyPatch("HasTrait")]
    public static class HarmonyPathces_HasTrait
    {
        [HarmonyPostfix]
        private static void Postfix(TraitDef tDef, Pawn ___pawn, ref bool __result)
        {
            if (tDef == TraitDefOf.Asexual)
            {
                if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.raceSexuality == 1)
                {
                    __result = true;
                    return;
                }
            }
            if (tDef == TraitDefOf.Bisexual)
            {
                if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.raceSexuality == 2)
                {
                    __result = true;
                    return;
                }
            }
            if (tDef == TraitDefOf.Gay)
            {
                if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.raceSexuality == 3)
                {
                    __result = true;
                    return;
                }
            }
        }
    }
    [HarmonyPatch(typeof(HediffSet))]
    [HarmonyPatch("HasHead", MethodType.Getter)]
    public static class Patch_HasHead
    {
        [HarmonyPrefix]
        private static bool Prefix(HediffSet __instance, bool? ___cachedHasHead, ref bool __result)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                bool? flag = ___cachedHasHead;
                if (!flag.HasValue)
                {
                    ___cachedHasHead = new bool?(__instance.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Any((BodyPartRecord x) => x.def == thingDef.raceAddonSettings.basicSetting.raceHeadDef));
                }
                __result = ___cachedHasHead.Value;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public static class Patch_PreApplyDamage
    {
        [HarmonyPrefix]
        private static bool Prefix(ref DamageInfo dinfo, Pawn __instance)
        {
            if (dinfo.Instigator is Pawn from && from != null && from.def is RaceAddonThingDef fromThingDef && from.CurJob.def == JobDefOf.SocialFight && fromThingDef.raceAddonSettings.basicSetting.maxDamageForSocialfight != 0)
            {
                dinfo.SetAmount(Mathf.Min(dinfo.Amount, fromThingDef.raceAddonSettings.basicSetting.maxDamageForSocialfight));
            }
            if (__instance.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.damageFactor != 1f)
            {
                dinfo.SetAmount(dinfo.Amount * thingDef.raceAddonSettings.healthSetting.damageFactor);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch("IsHumanlikeMeat")]
    public static class Patch_IsHumanlikeMeat
    {
        [HarmonyPostfix]
        private static void Postfix(ThingDef def, ref bool __result)
        {
            if (__result && def.ingestible.sourceDef is RaceAddonThingDef thingDef)
            {
                __result = thingDef.raceAddonSettings.basicSetting.humanlikeMeat;
            }
        }
    }
    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch("IsHumanlikeMeatOrHumanlikeCorpse")]
    public static class Patch_IsHumanlikeMeatOrHumanlikeCorpse
    {
        [HarmonyPostfix]
        private static void Postfix(Thing thing, ref bool __result)
        {
            if (__result && thing is Corpse corpse && corpse != null && corpse.InnerPawn.def is RaceAddonThingDef thingDef)
            {
                __result = thingDef.raceAddonSettings.basicSetting.humanlikeMeat;
            }
        }
    }
}