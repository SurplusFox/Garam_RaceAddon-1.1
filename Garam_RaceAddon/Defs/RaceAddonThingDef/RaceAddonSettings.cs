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
    public class RaceAddonSettings
    {
        public GraphicSetting graphicSetting = new GraphicSetting();
        public BasicSetting basicSetting = new BasicSetting();
        public HealthSetting healthSetting = new HealthSetting();
        public DeathSetting deathSetting = new DeathSetting();
        public RelationSetting relationSetting = new RelationSetting();
        public WorkSetting workSetting = new WorkSetting();
        public TraitSetting traitSetting = new TraitSetting();
        public MoodSetting moodSetting = new MoodSetting();
        public HediffSetting hediffSetting = new HediffSetting();
        public RaceRestriction<ThingDef> apparelRestrictionSetting = new RaceRestriction<ThingDef>();
        public RaceRestriction<ThingDef> weaponRestrictionSetting = new RaceRestriction<ThingDef>();
        public RaceRestriction<ThingDef> buildingRestrictionSetting = new RaceRestriction<ThingDef>();
        public RaceRestriction<ThingDef> foodRestrictionSetting = new RaceRestriction<ThingDef>();
        public RaceRestriction<ThingDef> plantRestrictionSetting = new RaceRestriction<ThingDef>();
        public RaceRestriction<ThingDef> animalRestrictionSetting = new RaceRestriction<ThingDef>();
        public RaceRestriction<RecipeDef> recipeRestrictionSetting = new RaceRestriction<RecipeDef>();
        public RaceRestriction<ResearchProjectDef> researchRestrictionSetting = new RaceRestriction<ResearchProjectDef>();
    }

    public class GraphicSetting
    {
        //Basic
        public ColorGenerator_Options rottingColor;
        public bool drawWound = true;
        //Animation
        public bool eyeBlink = false;
        public bool headAnimation = false;
        public bool headTargeting = false;
        //Draw
        public bool drawHat = true;
        public List<ThingDef> drawHat_Exceptions = new List<ThingDef>();
        public bool drawHair = true;
        public List<DrawHairOption> drawHair_Exceptions = new List<DrawHairOption>();
        public List<HideAddon> hideAddons;
        public List<DrawSize> drawSize = new List<DrawSize>();
        //Appearances
        public List<RaceAppearance> raceAppearances = new List<RaceAppearance>();

        public class DrawHairOption
        {
            public ThingDef thingDef;
            public int hairOption = 0;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
                hairOption = int.Parse(xmlRoot.FirstChild.Value);
            }
        }

        public class HideAddon
        {
            public ThingDef apparelDef;
            public List<AddonDef> addonDefs = new List<AddonDef>();
        }

        public class DrawSize
        {
            public int minAge = 0;
            public Vector2 headSize = new Vector2(1.0f, 1.0f);
            public Vector2 bodySize = new Vector2(1.0f, 1.0f);
            public Vector2 equipmentSize = new Vector2(1.0f, 1.0f);
            public List<ThingDef> allowedApparels = null;
            public List<ThingDef> allowedWeapons = null;
            public List<RaceAppearance> apparanceOverwrite = new List<RaceAppearance>();
            public bool hideHat = false;
        }

        public class RaceAppearance
        {
            public Gender gender;
            public AppearanceDef appearanceDef;
            public float weight = 1.0f;
        }
    }

    public class BasicSetting
    {
        public float maleChance = 0.5f;
        public int raceSexuality = 0;
        public BodyPartDef raceHeadDef = BodyPartDefOf.Head;
        public bool humanlikeMeat = true;
        public bool humanRecipeImport = false;
        public int maxDamageForSocialfight = 6;
    }

    public class HealthSetting
    {
        public int greyHairAt = 40;
        public bool antiAging = false;
        public bool dropBloodFilth = true;
        public float healingFactor = 1.0f;
        public float damageFactor = 1.0f;
        public float painFactor = 1.0f;
    }

    public class DeathSetting
    {
        public ImmediateDeath immediateDeath = new ImmediateDeath();
        public bool rottingCorpse = true;
        public Leavings leavings = new Leavings();

        public class ImmediateDeath
        {
            public FactionCheck playerFaction = new FactionCheck();
            public FactionCheck friendlyFaction = new FactionCheck();
            public FactionCheck hostileFaction = new FactionCheck();
            public FactionCheck noneFaction = new FactionCheck();
            public class FactionCheck
            {
                public bool inPainShock = false;
                public bool unconscious = false;
                public bool immovable = false;
            }
        }

        public class Leavings
        {
            public bool corpse = true;
            public bool equipment = true;
            public bool apparel = true;
            public List<Item> items;
            public class Item
            {
                public ThingDef thingDef;
                public bool useMeatAmount = false;
                public IntRange stackCount;
                public float chance = 1.0f;
            }
        }
    }

    public class RelationSetting
    {
        public bool randomRelationAllow = true;
        public List<ThingDef> sameRaces = new List<ThingDef>();
        public Non_TragetRace sameRace = new Non_TragetRace();
        public Non_TragetRace otherRace = new Non_TragetRace();
        public List<TragetRace> specialRaces = new List<TragetRace>();

        public class Non_TragetRace
        {
            public float compatibilityBonus = 0f;
            public LoveChanceFactor loveChanceFactor_Male;
            public LoveChanceFactor loveChanceFactor_Female;
        }
        public class TragetRace
        {
            public ThingDef raceDef;
            public float compatibilityBonus = 0f;
            public LoveChanceFactor loveChanceFactor_Male;
            public LoveChanceFactor loveChanceFactor_Female;
        }
        public class LoveChanceFactor
        {
            public float minAgeValue = 0.15f;
            public int minAgeDifference = -10;
            public int lowerAgeDifference = -4;
            public int upperAgeDifference = 4;
            public int maxAgeDifference = 10;
            public float maxAgeValue = 0.15f;
            public float finalCorrectionValue = 0f;
        }
    }

    public class WorkSetting
    {
        public RaceRestriction<WorkGiverDef> workGiverRestriction = new RaceRestriction<WorkGiverDef>();
        public List<WorkTags> workDisables;
        public List<SkillGain> skillGains;
    }

    public class TraitSetting
    {
        public IntRange traitCount = new IntRange(2, 3);
        public List<TraitInfo> forcedTraits = new List<TraitInfo>();
        public RaceRestriction<TraitInfo> traitRestriction = new RaceRestriction<TraitInfo>();

        public class TraitInfo
        {
            public TraitDef traitDef;
            public int degree = 0;
            public float chance = 1.0f;
        }
    }

    public class MoodSetting
    {
        public List<ReplacedMood> replacedMoods;
        public RaceRestriction<ThoughtDef> moodRestriction = new RaceRestriction<ThoughtDef>();

        public class ReplacedMood
        {
            public ThoughtDef originalThoughtDef;
            public ThoughtDef replacedThoughtDef;
        }
    }

    public class HediffSetting
    {
        public List<HediffWithChance> forcedHediffs = new List<HediffWithChance>();
        public List<ReplacedHediff> replacedHediffs;
        public RaceRestriction<HediffDef> hediffRestriction = new RaceRestriction<HediffDef>();

        public class HediffWithChance
        {
            public HediffDef hediffDef;
            public float severity = 0.0f;
            public float chance = 1.0f;
        }

        public class ReplacedHediff
        {
            public HediffDef originalHediffDef;
            public HediffDef replacedHediffDef;
        }
    }

    public class RaceRestriction<T>
    {
        public bool allAllow = true;
        public List<T> allAllow_Exceptions = new List<T>();
        public List<T> raceSpecifics = new List<T>();
    }
}
