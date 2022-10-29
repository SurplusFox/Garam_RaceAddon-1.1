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
    public class FaceDef : Def
    {
        public ShaderTypeDef shaderType;
        //Normally Path By Mood
        [NoTranslate]
        public string mentalBreakPath;
        [NoTranslate]
        public string aboutToBreakPath;
        [NoTranslate]
        public string onEdgePath;
        [NoTranslate]
        public string stressedPath;
        [NoTranslate]
        public string neutralPath;
        [NoTranslate]
        public string contentPath;
        [NoTranslate]
        public string happyPath;
        //Special Path
        [NoTranslate]
        public string sleepingPath;
        [NoTranslate]
        public string painShockPath;
        [NoTranslate]
        public string deadPath;
        [NoTranslate]
        public string blinkPath;
        [NoTranslate]
        public string winkPath;
        [NoTranslate]
        public string damagedPath;
        [NoTranslate]
        public string draftedPath;
        [NoTranslate]
        public string attackingPath;

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
