using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(FloatMenuUtility))]
    [HarmonyPatch("DecoratePrioritizedTask")]
    public static class HarmonyPatches_DecoratePrioritizedTask
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, LocalTargetInfo target, ref FloatMenuOption __result)
        {
            if (pawn != null && target.Thing != null)
            {
                var thing = target.Thing;
                if (thing is Apparel apparel && !RaceAddonTools.CheckApparel(pawn, apparel.def) && __result.Label == "ForceWear".Translate(apparel.LabelShort, apparel))
                {
                    __result = new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "RaceAddonRestriction_FloatMenu".Translate() + ")", null);
                }
                if (thing.TryGetComp<CompEquippable>() != null && !RaceAddonTools.CheckWeapon(pawn, thing.def) && __result.Label.Contains("Equip".Translate(thing.LabelShort)))
                {
                    __result = new FloatMenuOption("CannotEquip".Translate(thing.LabelShort) + " (" + "RaceAddonRestriction_FloatMenu".Translate() + ")", null);
                }
            }
        }
    }
    /*
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddHumanlikeOrders")]
    public static class HarmonyPatches_AddHumanlikeOrders
    {
        [HarmonyPriority(int.MinValue)]
        [HarmonyPostfix]
        private static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            var c = IntVec3.FromVector3(clickPos);
            if (pawn.apparel != null)
            {
                var list = c.GetThingList(pawn.Map).FindAll((Thing x) => x.def.IsApparel);
                foreach (Thing apparel in list)
                {
                    if (!RaceAddonTools.CheckApparel(pawn, apparel.def))
                    {
                        int i = opts.FindIndex((FloatMenuOption x) => !x.Disabled && x.Label.Contains("ForceWear".Translate(apparel.LabelShort, apparel)));
                        opts[i] = new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "RaceAddonRestriction_FloatMenu".Translate() + ")", null);
                    }
                }
            }
            if (pawn.equipment != null)
            {
                var list = c.GetThingList(pawn.Map).FindAll((Thing x) => x.TryGetComp<CompEquippable>() != null);
                foreach (Thing equipment in list)
                {
                    if (!RaceAddonTools.CheckWeapon(pawn, equipment.def))
                    {
                        int i = opts.FindIndex((FloatMenuOption x) =>  !x.Disabled && x.Label.Contains("Equip".Translate(equipment.LabelShort, equipment)));
                        opts[i] = new FloatMenuOption("CannotEquip".Translate(equipment.LabelShort) + " (" + "RaceAddonRestriction_FloatMenu".Translate(pawn.LabelShort, pawn) + ")", null);
                    }
                }
            }
        }
    }
    */
}
