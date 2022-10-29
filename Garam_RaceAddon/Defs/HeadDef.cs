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
    public class HeadDef : Def
    {
        public CrownType crownType = CrownType.Average;
        //Texture
        public ShaderTypeDef shaderType;
        [NoTranslate]
        public string replacedHeadPath;
        [NoTranslate]
        public string replacedSkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        [NoTranslate]
        public string replacedStumpPath = "Things/Pawn/Humanlike/Heads/None_Average_Stump";
        //Position
        public Offsets headTargetingOffsets = new Offsets();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (shaderType == null)
            {
                shaderType = ShaderTypeDefOf.Cutout;
            }
        }
    }
}
