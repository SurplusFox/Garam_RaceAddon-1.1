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
    public class BodyDef : Def
    {
        public BodyTypeDef bodyTypeDef;
        //Texture
        public ShaderTypeDef shaderType;
        [NoTranslate]
        public string replacedBodyPath;
        [NoTranslate]
        public string replacedSkullPath;

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
