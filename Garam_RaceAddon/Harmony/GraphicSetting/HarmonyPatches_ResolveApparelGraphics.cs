using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGraphicSet))]
    [HarmonyPatch("ResolveApparelGraphics")]
    public class HarmonyPatches_ResolveApparelGraphics
    {
        [HarmonyPostfix]
        private static void Postfix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                Pawn pawn = __instance.pawn;
                RaceAddonComp racomp = pawn.GetComp<RaceAddonComp>();

                if (pawn.apparel.WornApparel.Find((Apparel x) => x.def.apparel.LastLayer == ApparelLayerDefOf.Overhead) is Apparel apparel)
                {
                    ResolveHatDraw(racomp, thingDef, apparel);
                    ResolveHairDraw(racomp, thingDef, apparel);
                }

                ResolveAddonDraw(racomp, thingDef, racomp.AllAddonGraphicSets, pawn.apparel.WornApparel);
            }
        }
        private static void ResolveHatDraw(RaceAddonComp racomp, RaceAddonThingDef thingDef, Apparel apparel)
        {
            if (thingDef.raceAddonSettings.graphicSetting.drawSize[racomp.drawSize].hideHat)
            {
                racomp.drawHat = false;
                return;
            }
            if (thingDef.raceAddonSettings.graphicSetting.drawHat)
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawHat_Exceptions.Contains(apparel.def))
                {
                    racomp.drawHat = false;
                }
                else
                {
                    racomp.drawHat = true;
                }
            }
            else
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawHat_Exceptions.Contains(apparel.def))
                {
                    racomp.drawHat = true;
                }
                else
                {
                    racomp.drawHat = false;
                }
            }
        }
        private static void ResolveHairDraw(RaceAddonComp racomp, RaceAddonThingDef thingDef, Apparel apparel)
        {
            if (racomp.drawHat)
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawHair)
                {
                    if (thingDef.raceAddonSettings.graphicSetting.drawHair_Exceptions.Find(x => x.thingDef == apparel.def) is var preset && preset != null)
                    {
                        if (preset.hairOption == 0)
                        {
                            racomp.drawUpperHair = false;
                            racomp.drawLowerHair = false;
                        }
                        else if (preset.hairOption == 1)
                        {
                            racomp.drawUpperHair = false;
                            racomp.drawLowerHair = true;
                        }
                        else if (preset.hairOption == 2)
                        {
                            racomp.drawUpperHair = true;
                            racomp.drawLowerHair = false;
                        }
                    }
                    else
                    {
                        racomp.drawUpperHair = true;
                        racomp.drawLowerHair = true;
                    }
                }
                else
                {
                    if (thingDef.raceAddonSettings.graphicSetting.drawHair_Exceptions.Find(x => x.thingDef == apparel.def) is var preset && preset != null)
                    {
                        if (preset.hairOption == 0)
                        {
                            racomp.drawUpperHair = true;
                            racomp.drawLowerHair = true;
                        }
                        else if (preset.hairOption == 1)
                        {
                            racomp.drawUpperHair = true;
                            racomp.drawLowerHair = false;
                        }
                        else if (preset.hairOption == 2)
                        {
                            racomp.drawUpperHair = false;
                            racomp.drawLowerHair = true;
                        }
                    }
                    else
                    {
                        racomp.drawUpperHair = false;
                        racomp.drawLowerHair = false;
                    }
                }
            }
            else
            {
                racomp.drawUpperHair = true;
                racomp.drawLowerHair = true;
            }
        }
        internal static void ResolveAddonDraw(RaceAddonComp racomp, RaceAddonThingDef thingDef, List<AddonGraphicSet> addons, List<Apparel> apparels)
        {
            if (addons.Count > 0)
            {
                /*
                foreach (var set in addons)
                {
                    if (set.data.def.linkedBodyPart == "None" || racomp.Pawn.health.hediffSet.GetNotMissingParts().Any(x => x.untranslatedCustomLabel == set.data.def.linkedBodyPart || x.def.defName == set.data.def.linkedBodyPart))
                    {
                        set.draw = true;
                    }
                    else
                    {
                        set.draw = false;
                    }
                }
                */
                addons.ForEach(x => x.draw = true);
                if (apparels != null && thingDef.raceAddonSettings.graphicSetting.hideAddons != null && thingDef.raceAddonSettings.graphicSetting.hideAddons.Count > 0)
                {
                    foreach (Apparel apparel in apparels)
                    {
                        if (thingDef.raceAddonSettings.graphicSetting.hideAddons.Find(x => x.apparelDef == apparel.def) is var set && set != null)
                        {
                            var list2 = addons.FindAll(x => set.addonDefs.Contains(x.data.def));
                            if (list2 != null && list2.Count > 0)
                            {
                                list2.ForEach(x => x.draw = false);
                            }
                        }
                    }
                }
            }
        }
    }
}