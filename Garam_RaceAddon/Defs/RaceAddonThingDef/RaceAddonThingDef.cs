using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Garam_RaceAddon
{
    public class RaceAddonThingDef : ThingDef
    {
        public RaceAddonSettings raceAddonSettings = new RaceAddonSettings();

        public WorkTags DisabledWorkTags { private set; get; } = WorkTags.None;

        public List<WorkTypeDef> DisabledWorkTypes = new List<WorkTypeDef>();

        public List<WorkGiverDef> DisabledWorkGivers = new List<WorkGiverDef>();

        public string RaceStory = null;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            RaceAddonTools.AllRaceAddonThingDefs.Add(this);
            //==================== Rotting Corpse ====================//
            if (!raceAddonSettings.deathSetting.rottingCorpse)
            {
                race.corpseDef.comps.RemoveAll((CompProperties x) => x is CompProperties_Rottable);
                race.corpseDef.comps.RemoveAll((CompProperties x) => x is CompProperties_SpawnerFilth);
            }
            //==================== Work Setting ====================//
            if (raceAddonSettings.workSetting.workGiverRestriction.allAllow)
            {
                foreach (WorkGiverDef def in raceAddonSettings.workSetting.workGiverRestriction.allAllow_Exceptions)
                {
                    if (!DisabledWorkGivers.Contains(def))
                    {
                        DisabledWorkGivers.Add(def);
                    }
                }
            }
            else
            {
                foreach (WorkGiverDef def in DefDatabase<WorkGiverDef>.AllDefs)
                {
                    if (!raceAddonSettings.workSetting.workGiverRestriction.allAllow_Exceptions.Contains(def))
                    {
                        if (!DisabledWorkGivers.Contains(def))
                        {
                            DisabledWorkGivers.Add(def);
                        }
                    }
                }
            }
            foreach (WorkGiverDef def in raceAddonSettings.workSetting.workGiverRestriction.raceSpecifics)
            {
                if (!RaceAddonTools.WorkGiverRestrictions.Contains(def))
                {
                    RaceAddonTools.WorkGiverRestrictions.Add(def);
                }
            }
            if (raceAddonSettings.workSetting.workDisables != null)
            {
                WorkTags workTags = WorkTags.None;
                foreach (WorkTags tag in raceAddonSettings.workSetting.workDisables)
                {
                    workTags |= tag;
                }
                DisabledWorkTags = workTags;


                foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefsListForReading.FindAll(x => (DisabledWorkTags & x.workTags) != 0))
                {
                    if (!DisabledWorkTypes.Contains(def))
                    {
                        DisabledWorkTypes.Add(def);
                    }
                }
            }
            //==================== Trait Setting ====================//
            foreach (var info in raceAddonSettings.traitSetting.traitRestriction.raceSpecifics)
            {
                TraitEntry traitEntry = new TraitEntry(info.traitDef, info.degree);
                if (!RaceAddonTools.TraitRestrictions.Contains(traitEntry))
                {
                    RaceAddonTools.TraitRestrictions.Add(traitEntry);
                }
            }
            //==================== Mood Setting ====================//
            List<ThoughtDef> privateMoodList = raceAddonSettings.moodSetting.moodRestriction.raceSpecifics;
            foreach (ThoughtDef mood in privateMoodList)
            {
                if (!RaceAddonTools.MoodRestrictions.Contains(mood))
                {
                    RaceAddonTools.MoodRestrictions.Add(mood);
                }
            }
            //==================== Hediff Setting ====================//
            List<HediffDef> privateHediffList = raceAddonSettings.hediffSetting.hediffRestriction.raceSpecifics;
            foreach (HediffDef hediffDef in privateHediffList)
            {
                if (!RaceAddonTools.HediffRestrictions.Contains(hediffDef))
                {
                    RaceAddonTools.HediffRestrictions.Add(hediffDef);
                }
            }
            //==================== Restriction Setting ====================//
            foreach (ThingDef def in raceAddonSettings.apparelRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.ApparelRestrictions.Contains(def))
                {
                    RaceAddonTools.ApparelRestrictions.Add(def);
                }
            }
            foreach (ThingDef def in raceAddonSettings.weaponRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.WeaponRestrictions.Contains(def))
                {
                    RaceAddonTools.WeaponRestrictions.Add(def);
                }
            }
            foreach (ThingDef def in raceAddonSettings.buildingRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.BuildingRestrictions.Contains(def))
                {
                    RaceAddonTools.BuildingRestrictions.Add(def);
                }
            }
            foreach (ThingDef def in raceAddonSettings.foodRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.FoodRestrictions.Contains(def))
                {
                    RaceAddonTools.FoodRestrictions.Add(def);
                }
            }
            foreach (ThingDef def in raceAddonSettings.plantRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.PlantRestrictions.Contains(def))
                {
                    RaceAddonTools.PlantRestrictions.Add(def);
                }
            }
            foreach (ThingDef def in raceAddonSettings.animalRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.AnimalRestrictions.Contains(def))
                {
                    RaceAddonTools.AnimalRestrictions.Add(def);
                }
            }
            foreach (RecipeDef def in raceAddonSettings.recipeRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.RecipeRestrictions.Contains(def))
                {
                    RaceAddonTools.RecipeRestrictions.Add(def);
                }
            }
            foreach (ResearchProjectDef def in raceAddonSettings.researchRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddonTools.ResearchRestrictions.Contains(def))
                {
                    RaceAddonTools.ResearchRestrictions.Add(def);
                }
            }
            if (comps.Any(x => x.compClass == typeof(RaceAddonComp)))
            {
                Log.Warning("====================================================================================================");
                Log.Warning("[Garam, Race Addon] " + defName + " has a duplicate RaceAddonComp!");
                Log.Warning("====================================================================================================");
            }
            else
            {
                comps.Add(new CompProperties(typeof(RaceAddonComp)));
            }
        }
    }
}
