using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Garam_RaceAddon
{
    public class SimpleBackstoryDef : Def
    {
        public BackstorySlot slot;
        public string backstoryTitle;
        public string backstoryTitleShort;
        public string backstoryDescription;
        public List<string> spawnCategories = new List<string>();

        public List<WorkTags> workDisables = new List<WorkTags>();
        public List<WorkTags> requiredWorkTags = new List<WorkTags>();
        public List<TraitSetting.TraitInfo> forcedTraits = new List<TraitSetting.TraitInfo>();
        public List<TraitSetting.TraitInfo> disallowedTraits = new List<TraitSetting.TraitInfo>();
        public List<SkillGain> skillGains = new List<SkillGain>();

        public List<IngestionOutcomeDoer_GiveHediff> forcedHediffs;
        public IntRange bioAgeRange;
        public IntRange chronoAgeRange;

        public float maleChance = -1f;
        public List<GraphicSetting.RaceAppearance> raceAppearances;

        public Backstory backstory;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            WorkTags workDisables = WorkTags.None;
            if (this.workDisables != null)
            {
                this.workDisables.ForEach(x => workDisables |= x);
            }

            WorkTags requiredWorkTags = WorkTags.None;
            if (this.requiredWorkTags != null)
            {
                this.requiredWorkTags.ForEach(x => requiredWorkTags |= x);
            }

            List<TraitEntry> forcedTraits = new List<TraitEntry>();
            if (this.forcedTraits != null)
            {
                this.forcedTraits.ForEach(x => forcedTraits.Add(new TraitEntry(x.traitDef, x.degree)));
            }

            List<TraitEntry> disallowedTraits = new List<TraitEntry>();
            if (this.disallowedTraits != null)
            {
                this.disallowedTraits.ForEach(x => disallowedTraits.Add(new TraitEntry(x.traitDef, x.degree)));
            }

            Dictionary<SkillDef, int> skillGainsResolved = new Dictionary<SkillDef, int>();
            if (skillGains != null)
            {
                skillGains.ForEach(x => skillGainsResolved.Add(x.skill, x.xp));
            }

            backstory = new Backstory
            {
                identifier = defName,

                slot = slot,
                title = backstoryTitle,
                titleShort = backstoryTitleShort,
                baseDesc = backstoryDescription,
                spawnCategories = spawnCategories,

                workDisables = workDisables,
                requiredWorkTags = requiredWorkTags,
                forcedTraits = forcedTraits,
                disallowedTraits = disallowedTraits,
                skillGainsResolved = skillGainsResolved
            };
            Traverse.Create(backstory).Field("bodyTypeMaleResolved").SetValue(BodyTypeDefOf.Male);
            Traverse.Create(backstory).Field("bodyTypeFemaleResolved").SetValue(BodyTypeDefOf.Female);
            backstory.ResolveReferences();
            backstory.PostLoad();

            IEnumerable<string> errors;
            if (!(errors = backstory.ConfigErrors(false)).Any())
            {
                BackstoryDatabase.AddBackstory(backstory);
            }
            else
            {
                Log.Error("====================================================================================================");
                Log.Error("[Garam, Race Addon] BackstoryDef has errors : " + defName + string.Join("\n", errors.ToArray()));
                Log.Error("====================================================================================================");
            }
        }
    }
}