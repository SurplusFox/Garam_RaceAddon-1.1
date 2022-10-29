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
    [HarmonyPatch(typeof(Pawn_StoryTracker))]
    [HarmonyPatch("SkinColor", MethodType.Getter)]
    public static class HarmonyPatches_SkinColor
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn ___pawn, ref Color __result)
        {
            if (___pawn.def is RaceAddonThingDef)
            {
                RaceAddonComp racomp = ___pawn.GetComp<RaceAddonComp>();
                if (racomp.savedSkinData != null)
                {
                    __result = racomp.savedSkinData.color1;
                }
                else
                {
                    __result = Color.white;
                }
                return false;
            }
            return true;
        }
    }
}
