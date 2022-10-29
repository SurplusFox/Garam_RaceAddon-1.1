using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public static class RaceAddonTools
    {
        public static List<RaceAddonThingDef> AllRaceAddonThingDefs { internal set; get; } = new List<RaceAddonThingDef>();
        public static bool IsSlave(RaceAddonPawnKindDef def)
        {
            if (DefDatabase<RaceAddonPawnKindDef>.AllDefs.First(x => x == def) is var pkd)
            {
                if (pkd.pawnKindDefReplacement.Any(x => x.originalPawnKindDef == PawnKindDefOf.Slave))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsWildMan(RaceAddonPawnKindDef def)
        {
            if (DefDatabase<RaceAddonPawnKindDef>.AllDefs.First(x => x == def) is var pkd)
            {
                if (pkd.pawnKindDefReplacement.Any(x => x.originalPawnKindDef == PawnKindDefOf.WildMan))
                {
                    return true;
                }
            }
            return false;
        }

        public static PawnKindDef RandomPawnKindDefFromRaceAddonPawnKindDef(PawnKindDef kindDef)
        {
            var list = DefDatabase<RaceAddonPawnKindDef>.AllDefsListForReading.FindAll(x => x.pawnKindDefReplacement.Any(y => y.originalPawnKindDef == kindDef));
            if (list.Count > 0)
            {
                var list2 = new List<Pair<PawnKindDef, float>> { new Pair<PawnKindDef, float>(kindDef, 10f) };
                foreach (var rapkd in list)
                {
                    var set = rapkd.pawnKindDefReplacement.Find(x => x.originalPawnKindDef == kindDef);
                    list2.Add(new Pair<PawnKindDef, float>(rapkd, set.weight));
                    if (list2[0].Second > set.originalWeight)
                    {
                        list2[0] = new Pair<PawnKindDef, float>(kindDef, set.originalWeight);
                    }
                }
                return list2.RandomElementByWeight(x => x.Second).First;
            }
            return kindDef;
        }

        public static int GetPawnDrawSize(Pawn pawn, RaceAddonThingDef thingDef)
        {
            List<GraphicSetting.DrawSize> list = thingDef.raceAddonSettings.graphicSetting.drawSize;
            int result = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].minAge <= pawn.ageTracker.AgeBiologicalYearsFloat)
                {
                    result = i;
                }
            }
            return result;
        }
        public static List<WorkGiverDef> WorkGiverRestrictions { internal set; get; } = new List<WorkGiverDef>();
        public static bool CheckWorkGiver(ThingDef pawn, WorkGiverDef def)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.workSetting.workGiverRestriction;
                if (!WorkGiverRestrictions.Contains(def))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(def))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(def))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(def))
                {
                    return true;
                }
            }
            else
            {
                if (!WorkGiverRestrictions.Contains(def))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<HediffDef> HediffRestrictions { internal set; get; } = new List<HediffDef>();
        public static bool CheckHediff(ThingDef pawn, HediffDef def)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.hediffSetting.hediffRestriction;
                if (!HediffRestrictions.Contains(def))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(def))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(def))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(def))
                {
                    return true;
                }
            }
            else
            {
                if (!HediffRestrictions.Contains(def))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<ThoughtDef> MoodRestrictions { internal set; get; } = new List<ThoughtDef>();
        public static bool CheckMood(ThingDef pawn, ThoughtDef def)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.moodSetting.moodRestriction;
                if (!MoodRestrictions.Contains(def))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(def))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(def))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(def))
                {
                    return true;
                }
            }
            else
            {
                if (!MoodRestrictions.Contains(def))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<TraitEntry> TraitRestrictions { internal set; get; } = new List<TraitEntry>();
        public static bool CheckTrait(ThingDef pawn, TraitEntry traitEntry)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                if (!CheckRaceSexuality(thingDef, traitEntry.def))
                {
                    return false;
                }
                var set = thingDef.raceAddonSettings.traitSetting.traitRestriction;
                if (!TraitRestrictions.Contains(traitEntry))
                {
                    if (set.allAllow && !set.allAllow_Exceptions.Any(x => x.traitDef == traitEntry.def && x.degree == traitEntry.degree))
                    {
                        return true;
                    }
                    if (!set.allAllow && set.allAllow_Exceptions.Any(x => x.traitDef == traitEntry.def && x.degree == traitEntry.degree))
                    {
                        return true;
                    }
                }
                else if (set.raceSpecifics.Any(x => x.traitDef == traitEntry.def && x.degree == traitEntry.degree))
                {
                    return true;
                }
            }
            else if (!TraitRestrictions.Contains(traitEntry))
            {
                return true;
            }
            return false;
        }
        public static bool CheckTrait(ThingDef pawn, TraitDef def, int degree)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                if (!CheckRaceSexuality(thingDef, def))
                {
                    return false;
                }
                var set = thingDef.raceAddonSettings.traitSetting.traitRestriction;
                if (!TraitRestrictions.Any(x => x.def == def && x.degree == degree))
                {
                    if (set.allAllow && !set.allAllow_Exceptions.Any(x => x.traitDef == def && x.degree == degree))
                    {
                        return true;
                    }
                    if (!set.allAllow && set.allAllow_Exceptions.Any(x => x.traitDef == def && x.degree == degree))
                    {
                        return true;
                    }
                }
                else if (set.raceSpecifics.Any(x => x.traitDef == def && x.degree == degree))
                {
                    return true;
                }
            }
            else if (!TraitRestrictions.Any(x => x.def == def && x.degree == degree))
            {
                return true;
            }
            return false;
        }
        public static bool CheckTrait(ThingDef pawn, Trait trait)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                if (!CheckRaceSexuality(thingDef, trait.def))
                {
                    return false;
                }
                var set = thingDef.raceAddonSettings.traitSetting.traitRestriction;
                if (!TraitRestrictions.Any(x => x.def == trait.def && x.degree == trait.Degree))
                {
                    if (set.allAllow && !set.allAllow_Exceptions.Any(x => x.traitDef == trait.def && x.degree == trait.Degree))
                    {
                        return true;
                    }
                    if (!set.allAllow && set.allAllow_Exceptions.Any(x => x.traitDef == trait.def && x.degree == trait.Degree))
                    {
                        return true;
                    }
                }
                else if (set.raceSpecifics.Any(x => x.traitDef == trait.def && x.degree == trait.Degree))
                {
                    return true;
                }
            }
            else if (!TraitRestrictions.Any(x => x.def == trait.def && x.degree == trait.Degree))
            {
                return true;
            }
            return false;
        }
        private static bool CheckRaceSexuality(RaceAddonThingDef thingDef, TraitDef def)
        {
            if (thingDef.raceAddonSettings.basicSetting.raceSexuality != 0)
            {
                if (def == TraitDefOf.Asexual || def == TraitDefOf.Bisexual || def == TraitDefOf.Gay)
                {
                    return false;
                }
            }
            return true;
        }
        public static List<ThingDef> ApparelRestrictions { internal set; get; } = new List<ThingDef>();
        public static List<ThingDef> WeaponRestrictions { internal set; get; } = new List<ThingDef>();
        public static List<ThingDef> BuildingRestrictions { internal set; get; } = new List<ThingDef>();
        public static List<ThingDef> FoodRestrictions { internal set; get; } = new List<ThingDef>();
        public static List<ThingDef> PlantRestrictions { internal set; get; } = new List<ThingDef>();
        public static List<ThingDef> AnimalRestrictions { internal set; get; } = new List<ThingDef>();
        public static List<RecipeDef> RecipeRestrictions { internal set; get; } = new List<RecipeDef>();
        public static List<ResearchProjectDef> ResearchRestrictions { internal set; get; } = new List<ResearchProjectDef>();
        public static bool CheckApparel(Pawn pawn, ThingDef apparel)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawSize[pawn.GetComp<RaceAddonComp>().drawSize].allowedApparels is var list && list != null)
                {
                    if (list.Contains(apparel))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                var set = thingDef.raceAddonSettings.apparelRestrictionSetting;
                if (!ApparelRestrictions.Contains(apparel))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(apparel))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(apparel))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(apparel))
                {
                    return true;
                }
            }
            else
            {
                if (!ApparelRestrictions.Contains(apparel))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckWeapon(Pawn pawn, ThingDef weapon)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawSize[pawn.GetComp<RaceAddonComp>().drawSize].allowedWeapons is var list && list != null)
                {
                    if (list.Contains(weapon))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                var set = thingDef.raceAddonSettings.weaponRestrictionSetting;
                if (!WeaponRestrictions.Contains(weapon))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(weapon))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(weapon))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(weapon))
                {
                    return true;
                }
            }
            else
            {
                if (!WeaponRestrictions.Contains(weapon))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckBuilding(ThingDef pawn, ThingDef building)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.buildingRestrictionSetting;
                if (!BuildingRestrictions.Contains(building))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(building))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(building))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(building))
                {
                    return true;
                }
            }
            else
            {
                if (!BuildingRestrictions.Contains(building))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckFood(ThingDef pawn, ThingDef food)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.foodRestrictionSetting;
                if (!FoodRestrictions.Contains(food))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(food))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(food))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(food))
                {
                    return true;
                }
            }
            else
            {
                if (!FoodRestrictions.Contains(food))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckPlant(ThingDef pawn, ThingDef plant)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.plantRestrictionSetting;
                if (!PlantRestrictions.Contains(plant))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(plant))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(plant))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(plant))
                {
                    return true;
                }
            }
            else
            {
                if (!PlantRestrictions.Contains(plant))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckAnimal(ThingDef pawn, ThingDef animal)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.animalRestrictionSetting;
                if (!AnimalRestrictions.Contains(animal))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(animal))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(animal))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(animal))
                {
                    return true;
                }
            }
            else
            {
                if (!AnimalRestrictions.Contains(animal))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckRecipe(ThingDef pawn, RecipeDef recipe)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.recipeRestrictionSetting;
                if (!RecipeRestrictions.Contains(recipe))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(recipe))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(recipe))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(recipe))
                {
                    return true;
                }
            }
            else
            {
                if (!RecipeRestrictions.Contains(recipe))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckResearch(ThingDef pawn, ResearchProjectDef research)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.researchRestrictionSetting;
                if (!ResearchRestrictions.Contains(research))
                {
                    if (set.allAllow)
                    {
                        if (!set.allAllow_Exceptions.Contains(research))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (set.allAllow_Exceptions.Contains(research))
                        {
                            return true;
                        }
                    }
                }
                else if (set.raceSpecifics.Contains(research))
                {
                    return true;
                }
            }
            else
            {
                if (!ResearchRestrictions.Contains(research))
                {
                    return true;
                }
            }
            return false;
        }
    }
}