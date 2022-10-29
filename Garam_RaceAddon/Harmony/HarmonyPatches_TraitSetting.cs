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
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateTraits")]
    public static class HarmonyPatches_GenerateTraits
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.story == null)
			{
				return false;
			}

			RaceAddonThingDef thingDef = pawn.def as RaceAddonThingDef;

			foreach (var backstory in pawn.story.AllBackstories)
			{
				List<TraitEntry> forcedTraits = backstory.forcedTraits;
				if (forcedTraits != null)
				{
					foreach (var traitEntry in forcedTraits)
					{
						if (traitEntry.def == null)
						{
							Log.Error("Null forced trait def on " + pawn.story.childhood, false);
						}
						else if (!pawn.story.traits.HasTrait(traitEntry.def) && RaceAddonTools.CheckTrait(pawn.def, traitEntry))
						{
							pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree, false));
						}
					}
				}
			}
			if (thingDef != null)
			{
				foreach (var set in thingDef.raceAddonSettings.traitSetting.forcedTraits)
				{
					if (Rand.Chance(set.chance))
					{
						if (set.traitDef == null)
						{
							Log.Error("Null forced trait def on " + thingDef.defName, false);
						}
						else if (!pawn.story.traits.HasTrait(set.traitDef) && RaceAddonTools.CheckTrait(pawn.def, set.traitDef, set.degree))
						{
							pawn.story.traits.GainTrait(new Trait(set.traitDef, set.degree, false));
						}
					}
				}
			}

			int traitCount = 0;
			if (thingDef != null)
			{
				traitCount = thingDef.raceAddonSettings.traitSetting.traitCount.RandomInRange;
			}
			else
			{
				traitCount = Rand.RangeInclusive(2, 3);
			}

			if (request.AllowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
			{
				Trait trait = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay), false);
				if (RaceAddonTools.CheckTrait(pawn.def, trait))
				{
					pawn.story.traits.GainTrait(trait);
				}
			}

			while (pawn.story.traits.allTraits.Count < traitCount)
			{
				TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
				int degree = PawnGenerator.RandomTraitDegree(newTraitDef);

				if (!pawn.story.traits.HasTrait(newTraitDef) && RaceAddonTools.CheckTrait(pawn.def, newTraitDef, degree))
				{
					if (newTraitDef == TraitDefOf.Gay)
					{
						if (!request.AllowGay)
						{
							continue;
						}
						if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
						{
							continue;
						}
					}
					if (request.Faction == null || Faction.OfPlayerSilentFail == null || !request.Faction.HostileTo(Faction.OfPlayer) || newTraitDef.allowOnHostileSpawn)
					{
						if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))))
						{
							if (newTraitDef.requiredWorkTypes == null || !pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
							{
								if (!pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
								{
									if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood == null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
									{
										Trait trait2 = new Trait(newTraitDef, degree, false);
										if (pawn.mindState != null && pawn.mindState.mentalBreaker != null)
										{
											float num2 = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
											num2 += trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold);
											num2 *= trait2.MultiplierOfStat(StatDefOf.MentalBreakThreshold);
											if (num2 > 0.4f)
											{
												continue;
											}
										}
										pawn.story.traits.GainTrait(trait2);
									}
								}
							}
						}
					}
				}
			}
			return false;
        }
    }
}