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
    public class AppearanceDef : Def
    {
        public List<ColorPalette> colorPalettes = new List<ColorPalette>();
        public List<SkinSet> skinList = new List<SkinSet>();
        public List<BodySet> bodyList = new List<BodySet>();
        public List<HeadSet> headList = new List<HeadSet>();
        public List<FaceSet> faceList = new List<FaceSet>();
        public List<HairSet> hairList = new List<HairSet>();
        public List<AddonSet> addonList = new List<AddonSet>();

        public class ColorPalette
        {
            public string paletteName;
            public ColorGenerator_Options color;
            public FloatRange melanin = new FloatRange(-1, -1);
        }
        public class ColorFromString
        {
            public string color1_PaletteName;
            public string color2_PaletteName;
        }
        public class SkinSet
        {
            public ColorFromString skinColor;
            public float weight = 1.0f;
        }
        public class BodySet
        {
            public BodyDef bodyDef;
            public float weight = 1.0f;
        }
        public class HeadSet
        {
            public HeadDef headDef;
            public float weight = 1.0f;
        }
        public class FaceSet
        {
            public FaceDef upperFaceDef;
            public FaceDef lowerFaceDef;
            public ColorFromString faceColor;
            public float weight = 1.0f;
        }
        public class HairSet
        {
            [NoTranslate]
            public List<string> hairTags;
            public ColorFromString hairColor;
            public float weight = 1.0f;
        }
        public class AddonSet
        {
            public AddonDef addonDef;
            public ColorFromString addonColor;
        }
    }
}
