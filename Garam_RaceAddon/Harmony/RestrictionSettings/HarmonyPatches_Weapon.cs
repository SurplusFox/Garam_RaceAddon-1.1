using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnWeaponGenerator))]
    [HarmonyPatch("TryGenerateWeaponFor")]
    public static class HarmonyPatches_TryGenerateWeaponFor
    {
        private static List<ThingStuffPair> savedAllWeaponPairs;
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, ref List<ThingStuffPair> ___allWeaponPairs)
        {
            if (savedAllWeaponPairs == null)
            {
                savedAllWeaponPairs = ___allWeaponPairs.ListFullCopy();
            }
            ___allWeaponPairs = savedAllWeaponPairs.FindAll((ThingStuffPair x) => RaceAddonTools.CheckWeapon(pawn, x.thing));
            return true;
        }
    }
}