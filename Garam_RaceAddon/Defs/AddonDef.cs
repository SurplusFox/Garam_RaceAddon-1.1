using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public class AddonDef : Def
    {
        //Texture
        public ShaderTypeDef shaderType;
        [NoTranslate]
        public string texturePath;
        public List<HediffPath> hediffPaths = new List<HediffPath>();
        //Draw
        [NoTranslate]
        public string linkedBodyPart = "None";
        public bool drawnInBed = false;
        public bool rotting = true;
        //Position
        public bool drawingToBody = true;
        public InFrontOfBody inFrontOfBody = new InFrontOfBody();
        public Offsets offsets = new Offsets();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (shaderType == null)
            {
                shaderType = ShaderTypeDefOf.Cutout;
            }
        }

        public class HediffPath
        {
            public HediffDef hediffDef;

            public string path;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if (xmlRoot.ChildNodes.Count != 1)
                {
                    Log.Error("Misconfigured HediffPath: " + xmlRoot.OuterXml, false);
                    return;
                }
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "hediffDef", xmlRoot.Name);
                this.path = xmlRoot.FirstChild.Value;
            }
        }
    }

    public class InFrontOfBody
    {
        public bool north = true;
        public bool south = true;
        public bool west = true;
        public bool east = true;
    }

    public class Offsets
    {
        public Vector3 north = Vector3.zero;
        public Vector3 south = Vector3.zero;
        public Vector3 west = Vector3.zero;
        public Vector3 east = Vector3.zero;
    }
}
