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
    public class RaceAddonComp : ThingComp
    {
        //non-save data
        public EyeBlinker eyeBlinker;
        public HeadRotator headRotator;
        public HeadTargeter headTargeter;

        public FaceGraphicSet lowerFaceGraphicSet;
        public FaceGraphicSet upperFaceGraphicSet;
        public List<AddonGraphicSet> bodyAddonGraphicSets;
        public List<AddonGraphicSet> headAddonGraphicSets;

        public List<AddonGraphicSet> AllAddonGraphicSets 
        {
            get
            {
                List<AddonGraphicSet> result = new List<AddonGraphicSet>();
                if (bodyAddonGraphicSets != null)
                {
                    bodyAddonGraphicSets.ForEach(x => result.Add(x));
                }
                if (headAddonGraphicSets != null)
                {
                    headAddonGraphicSets.ForEach(x => result.Add(x));
                }
                return result;
            }
        }

        public GraphicMeshSet headMeshSet;
        public GraphicMeshSet bodyMeshSet;
        public GraphicMeshSet equipmentMeshSet;

        public Graphic improvedHairGraphic;
        public bool drawHat = false;
        public bool drawUpperHair = true;
        public bool drawLowerHair = true;
        public override void CompTick()
        {
            base.CompTick();
            if (Pawn != null && Pawn.Spawned)
            {
                if (eyeBlinker != null)
                {
                    eyeBlinker.Check(Pawn.needs.mood.CurLevel);
                }
                if (headRotator != null)
                {
                    headRotator.Check();
                }
                if (headTargeter != null)
                {
                    headTargeter.Check();
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (Pawn != null && Pawn.Spawned)
            {
                // resolve draw size
                var newDrawSize = RaceAddonTools.GetPawnDrawSize(Pawn, ThingDef);
                if (drawSize != newDrawSize)
                {
                    drawSize = newDrawSize;
                    if (ThingDef.raceAddonSettings.graphicSetting.drawSize[drawSize].apparanceOverwrite.FindAll(x => x.gender == Gender.None || x.gender == Pawn.gender) is var overwrite && overwrite.Count > 0)
                    {
                        var appearanceDef = overwrite.RandomElementByWeight(x => x.weight).appearanceDef;
                        HarmonyPathces_PawnGenerator_GeneratePawn.GenerateAppearance(Pawn, appearanceDef);
                        Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                    else
                    {
                        var appearanceDef = ThingDef.raceAddonSettings.graphicSetting.raceAppearances.FindAll(x => x.gender == Gender.None || x.gender == Pawn.gender).RandomElementByWeight(x => x.weight).appearanceDef;
                        HarmonyPathces_PawnGenerator_GeneratePawn.GenerateAppearance(Pawn, appearanceDef);
                        Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                    if (Pawn.IsColonist)
                    {
                        Find.LetterStack.ReceiveLetter("RaceAddon_GrowUp_Label".Translate(Pawn.Name.ToStringShort), "RaceAddon_GrowUp_String".Translate(), LetterDefOf.PositiveEvent);
                    }
                }
                foreach (var apparel in Pawn.apparel.WornApparel)
                {
                    if (!RaceAddonTools.CheckApparel(Pawn, apparel.def))
                    {
                        Pawn.apparel.TryDrop(apparel);
                    }
                }
                HarmonyPatches_ResolveApparelGraphics.ResolveAddonDraw(this, ThingDef, AllAddonGraphicSets, null);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ThingDef = parent.def as RaceAddonThingDef;
            Pawn = parent as Pawn;
        }

        //save data
        public RaceAddonThingDef ThingDef { get; private set; }
        public Pawn Pawn { get; private set; }
        public SavedSkinData savedSkinData;
        public SavedBodyData savedBodyData;
        public SavedHeadData savedHeadData;
        public SavedFaceData savedFaceData;
        public SavedHairData savedHairData;
        public List<SavedAddonData> savedAddonDatas;
        public int drawSize = -1;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref savedSkinData, "savedSkinData");
            Scribe_Deep.Look(ref savedBodyData, "savedBodyData");
            Scribe_Deep.Look(ref savedHeadData, "savedHeadData");
            Scribe_Deep.Look(ref savedFaceData, "savedFaceData");
            Scribe_Deep.Look(ref savedHairData, "savedHairData");
            Scribe_Collections.Look(ref savedAddonDatas, "savedAddonDatas", LookMode.Deep);
            Scribe_Values.Look(ref drawSize, "drawSize");
        }

        public RaceAddonComp()
        {

        }
    }

    public class SavedSkinData : IExposable
    {
        public Color color1;
        public Color color2;
        public Color rottingColor;
        public void ExposeData()
        {
            Scribe_Values.Look(ref color1, "color1");
            Scribe_Values.Look(ref color2, "color2");
            Scribe_Values.Look(ref rottingColor, "rottingColor");
        }
    }

    public class SavedBodyData : IExposable
    {
        public BodyDef def;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class SavedHeadData : IExposable
    {
        public HeadDef def;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
        }
    }

    public class SavedFaceData : IExposable
    {
        public FaceDef upperDef;
        public FaceDef lowerDef;
        public Color color1;
        public Color color2;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref upperDef, "upperDef");
            Scribe_Defs.Look(ref lowerDef, "lowerDef");
            Scribe_Values.Look(ref color1, "color1");
            Scribe_Values.Look(ref color2, "color2");
        }
    }

    public class SavedHairData : IExposable
    {
        public Color color2;
        public void ExposeData()
        {
            Scribe_Values.Look(ref color2, "color2");
        }
    }

    public class SavedAddonData : IExposable
    {
        public AddonDef def;
        [NoTranslate]
        public string texturePath;
        public Color color1;
        public Color color2;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref texturePath, "texturePath");
            Scribe_Values.Look(ref color1, "color1");
            Scribe_Values.Look(ref color2, "color2");
        }
    }

    public class EyeBlinker
    {
        private int nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);

        private int currentTick = 0;
        private int tickForOverallAnimation = 0;

        private const int minWaitTick = 120;
        private const int maxWaitTick = 240;

        private const int minTickForAnimation = 30;
        private const int maxTickForAnimation = 60;

        private const float winkChance = 0.2f;

        public void Check(float mood)
        {
            if (!NowPlaying)
            {
                nextDelay--;
                if (nextDelay == 0)
                {
                    // start animation
                    tickForOverallAnimation = Rand.RangeInclusive(minTickForAnimation, maxTickForAnimation);
                    if (Rand.Chance(winkChance * mood))
                    {
                        WinkNow = true;
                    }
                    else
                    {
                        BlinkNow = true;
                    }
                }
            }
            else
            {
                currentTick++;
                if (currentTick > tickForOverallAnimation)
                {
                    // waiting tick
                    WinkNow = false;
                    BlinkNow = false;
                    currentTick = 0;
                    nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                }
            }
        }
        public bool NowPlaying
        {
            get
            {
                return nextDelay <= 0;
            }
        }
        public bool WinkNow { get; private set; } = false;
        public bool BlinkNow { get; private set; } = false;
    }

    public class HeadRotator // 꾸악님 고마워용!!!!
    {
        private int nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);

        private int currentTick = 0;
        private int tickForOverallAnimation = 0;    // T
        private float currentAnimationAngle = 0f;   // A
        private int rotateDirectionSign = 0;

        private const int minWaitTick = 60;
        private const int maxWaitTick = 600;

        private const int minTickForAnimation = 120;
        private const int maxTickForAnimation = 240;

        private const float minAngle = 2.0f;
        private const float maxAngle = 8.0f;

        public void Check()
        {
            if (!NowPlaying)
            {
                nextDelay--;
                if (nextDelay == 0)
                {
                    // start animation
                    rotateDirectionSign = Rand.Bool ? 1 : -1;
                    tickForOverallAnimation = Rand.RangeInclusive(minTickForAnimation, maxTickForAnimation) * 2;
                    currentAnimationAngle = minAngle + Rand.Value * (maxAngle - minAngle);
                }
            }
            else
            {
                currentTick++;
                if (currentTick > tickForOverallAnimation)
                {
                    // waiting tick
                    currentTick = 0;
                    nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                }
            }
        }
        public bool NowPlaying
        {
            get
            {
                return nextDelay <= 0;
            }
        }
        private float Angle
        {
            get
            {
                if (!NowPlaying)
                {
                    return 0f;
                }
                else if (currentTick < tickForOverallAnimation / 4)
                {
                    // f1 is solution of ODE like
                    // f1'(0) = 0, f1'(T/4) = 0, f1(0) = 0, f1(T/4) = A
                    // I choose 3th polynomial for my convenience. but there is many solutions.
                    return (-currentAnimationAngle / Mathf.Pow(tickForOverallAnimation, 3f) * Mathf.Pow((float)currentTick, 2)) * (128 * currentTick - 48 * tickForOverallAnimation) * rotateDirectionSign;
                }
                else if (currentTick < tickForOverallAnimation * 3 / 4)
                {
                    // f2 is solution of ODE like
                    // f2'(T/4) = 0, f2'(3T/4) = 0, f2(T/4) = A, f2(3T/4) = -A
                    // I choose A * sin(2pi*x / T) for my convenience. but there is many solutions.
                    return (currentAnimationAngle * Mathf.Sin(2f * Mathf.PI * (float)currentTick / (float)tickForOverallAnimation)) * rotateDirectionSign;
                }
                else
                {
                    // f3 is solution of ODE like
                    // f3'(3T/4) = 0, f3'(T) = 0, f3(3T/4) = -A, f3(T) = 0
                    // I choose 3th polynomial for my convenience. but there is many solutions.
                    return (currentAnimationAngle / Mathf.Pow(tickForOverallAnimation, 3f)) * (float)(80 * tickForOverallAnimation - 128 * currentTick) * Mathf.Pow(currentTick - tickForOverallAnimation, 2f) * rotateDirectionSign;
                }
            }
        }
        public Quaternion GetQuat()
        {
            return Quaternion.AngleAxis(Angle, Vector3.up);
        }
    }

    public class HeadTargeter
    {
        private int nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);

        private readonly Pawn pawn;
        private Pawn target;
        public RotationDirection RotDirection { get; private set; } = RotationDirection.None;

        private int currentTick = 0;
        private int tickForOverallAnimation = 0;

        private const int minWaitTick = 600;
        private const int maxWaitTick = 1800;

        private const int minTickForAnimation = 120;
        private const int maxTickForAnimation = 360;

        public HeadTargeter(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void Check()
        {
            if (!NowPlaying)
            {
                nextDelay--;
                if (nextDelay == 0)
                {
                    target = GetTarget();
                    if (target != null)
                    {
                        // start animation
                        tickForOverallAnimation = Rand.RangeInclusive(minTickForAnimation, maxTickForAnimation);
                        RotDirection = GetRotDiretion();
                    }
                    else
                    {
                        nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                    }
                }
            }
            else
            {
                currentTick++;
                if (currentTick % 20 == 0)
                {
                    RotDirection = GetRotDiretion();
                }
                if (currentTick > tickForOverallAnimation)
                {
                    // waiting tick
                    target = null;
                    RotDirection = RotationDirection.None;
                    currentTick = 0;
                    nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                }
            }
        }
        public bool NowPlaying
        {
            get
            {
                return nextDelay <= 0;
            }
        }
        private Pawn GetTarget()
        {
            List<Pawn> targets = new List<Pawn>();
            for (int i = 1; i < 57; i++)
            {
                IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(pawn.Map))
                {
                    Pawn target = intVec.GetFirstPawn(pawn.Map);
                    if (target != null && !target.Dead && !target.Downed && GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map))
                    {
                        if (target.HostileTo(pawn))
                        {
                            return null;
                        }
                        else
                        {
                            targets.Add(target);
                        }
                    }
                }
            }
            if (targets.Count > 0)
            {
                return targets.RandomElement();
            }
            return null;
        }
        private RotationDirection GetRotDiretion()
        {
            float angle = (target.Position - pawn.Position).ToVector3().AngleFlat();
            Rot4 rot = Pawn_RotationTracker.RotFromAngleBiased(angle);
            if (rot != pawn.Rotation.Opposite)
            {
                switch (pawn.Rotation.AsInt - rot.AsInt)
                {
                    case 0:
                        return RotationDirection.None;
                    case -1:
                        return RotationDirection.Clockwise;
                    case 1:
                        return RotationDirection.Counterclockwise;
                }
            }
            return RotationDirection.None;
        }
    }
}
