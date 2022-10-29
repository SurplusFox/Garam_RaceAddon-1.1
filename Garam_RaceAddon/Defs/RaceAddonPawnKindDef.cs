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
    public class RaceAddonPawnKindDef : PawnKindDef
    {
        public List<PawnKindDefReplacement> pawnKindDefReplacement = new List<PawnKindDefReplacement>();

        public bool onlyUseThisBackstoryCategoryes = false;

        public class PawnKindDefReplacement
        {
            public PawnKindDef originalPawnKindDef;
            public float originalWeight = 10f;
            public float weight = 10f;
        }
    }
}
