using RimWorld;
using Verse;
using System.Collections.Generic;
using HarmonyLib;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("AddHediff", new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class HarmonyPatches_AddHediff
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn ___pawn, Hediff hediff)
        {
            if (!RaceAddonTools.CheckHediff(___pawn.def, hediff.def))
            {
                return false;
            }
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.hediffSetting.replacedHediffs != null)
                {
                    var replacedHediff = thingDef.raceAddonSettings.hediffSetting.replacedHediffs.Find(x => x.originalHediffDef == hediff.def);
                    if (replacedHediff != null)
                    {
                        hediff.def = replacedHediff.replacedHediffDef;
                    }
                }
            }
            return true;
        }
        [HarmonyPostfix]
        private static void Postfix(Pawn ___pawn, Hediff hediff)
        {
            if (___pawn.GetComp<RaceAddonComp>() is var racomp && racomp != null && racomp.savedAddonDatas != null && racomp.savedAddonDatas.Count > 0)
            {
                foreach (var data in racomp.savedAddonDatas)
                {
                    if (data.def.hediffPaths.Any(x => x.hediffDef == hediff.def))
                    {
                        racomp.AllAddonGraphicSets.FindAll(x => x.data == data).ForEach(x => x.ResolveAllGraphics(racomp.savedSkinData.rottingColor, ___pawn.health.hediffSet));
                    }
                }
            }
        }
    }
}