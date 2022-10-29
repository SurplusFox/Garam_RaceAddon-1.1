using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(Selector))]
    [HarmonyPatch("Select")]
    public static class Patch_Test
    {
        [HarmonyPrefix]
        private static bool Prefix(object obj)
        {
            if (obj is Pawn pawn && pawn.RaceProps.Humanlike)
            {

            }
            return true;
        }

        [HarmonyPostfix]
        private static void Postfix(object obj)
        {
            if (obj is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                if (pawn.def is RaceAddonThingDef thingDef)
                {

                }
            }
        }
    }
}
