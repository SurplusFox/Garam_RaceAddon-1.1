using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class HarmonyPathces_PawnGenerator_GeneratePawn
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        private static bool Prefix(ref PawnGenerationRequest request)
        {
            if (!request.Newborn)
            {
                request = new PawnGenerationRequest(RaceAddonTools.RandomPawnKindDefFromRaceAddonPawnKindDef(request.KindDef), request.Faction,
                    request.Context, request.Tile, request.ForceGenerateNewPawn, request.Newborn,
                    request.AllowDead, request.AllowDowned, request.CanGeneratePawnRelations,
                    request.MustBeCapableOfViolence, request.ColonistRelationChanceFactor, request.ForceAddFreeWarmLayerIfNeeded,
                    request.AllowGay, request.AllowFood, request.AllowAddictions, request.Inhabitant,
                    request.CertainlyBeenInCryptosleep, request.ForceRedressWorldPawnIfFormerColonist, request.WorldPawnFactionDoesntMatter,
                    request.BiocodeWeaponChance, request.ExtraPawnForExtraRelationChance, request.RelationWithExtraPawnChanceFactor,
                    request.ValidatorPreGear, request.ValidatorPostGear, request.ForcedTraits,
                    request.ProhibitedTraits, request.MinChanceToRedressWorldPawn, request.FixedBiologicalAge,
                    request.FixedChronologicalAge, request.FixedGender, request.FixedMelanin, request.FixedLastName,
                    request.FixedBirthName, request.FixedTitle);
            }
            return true;
        }

        [HarmonyPostfix]
        private static void Postfix(ref Pawn __result, ref PawnGenerationRequest request)
        {
            RaceAddonComp racomp = null;
            if (__result.def is RaceAddonThingDef thingDef)
            {
                // get backstorydef
                SimpleBackstoryDef simpleBackstoryDef = null;
                foreach (var backstory in __result.story.AllBackstories)
                {
                    if (DefDatabase<SimpleBackstoryDef>.AllDefsListForReading.Find(x => x.defName == backstory.identifier) is var def && def != null)
                    {
                        simpleBackstoryDef = def;
                    }
                }

                // make default setting
                racomp = __result.GetComp<RaceAddonComp>();

                float maleChance = (simpleBackstoryDef != null && simpleBackstoryDef.maleChance >= 0) ? simpleBackstoryDef.maleChance : thingDef.raceAddonSettings.basicSetting.maleChance;
                __result.gender = GetGender(maleChance, request.FixedGender);
                var gender = __result.gender;

                __result.Name = PawnBioAndNameGenerator.GeneratePawnName(__result, NameStyle.Full, request.FixedLastName);

                racomp.drawSize = RaceAddonTools.GetPawnDrawSize(__result, thingDef);
                var drawSize = thingDef.raceAddonSettings.graphicSetting.drawSize[racomp.drawSize];

                AppearanceDef appearanceDef = null;
                var list = (simpleBackstoryDef != null && simpleBackstoryDef.raceAppearances != null) ? simpleBackstoryDef.raceAppearances : thingDef.raceAddonSettings.graphicSetting.raceAppearances;
                if (drawSize.apparanceOverwrite.FindAll(x => x.gender == Gender.None || x.gender == gender) is var list2 && list2.Count > 0)
                {
                    appearanceDef = list2.RandomElementByWeight(x => x.weight).appearanceDef;
                }
                else if (list.FindAll(x => x.gender == Gender.None || x.gender == gender) is var list3 && list3.Count > 0)
                {
                    appearanceDef = list3.RandomElementByWeight(x => x.weight).appearanceDef;
                }
                else
                {
                    Log.Error("[Garam, Race Addon] There is no appearanceDef selected.");
                    return;
                }

                GenerateAppearance(__result, appearanceDef);

                // work setting
                if (thingDef.raceAddonSettings.workSetting.skillGains != null)
                {
                    foreach (SkillGain skillGain in thingDef.raceAddonSettings.workSetting.skillGains)
                    {
                        __result.skills.Learn(skillGain.skill, skillGain.xp, true);
                        foreach (var skill in __result.skills.skills)
                        {
                            if (skill.def == skillGain.skill)
                            {
                                skill.Level += skillGain.xp;
                            }
                        }
                    }
                }

                // hediff setting
                if (thingDef.raceAddonSettings.hediffSetting.forcedHediffs != null)
                {
                    foreach (var set in thingDef.raceAddonSettings.hediffSetting.forcedHediffs)
                    {
                        new IngestionOutcomeDoer_GiveHediff
                        {
                            hediffDef = set.hediffDef,
                            severity = set.severity,
                            chance = set.chance
                        }.DoIngestionOutcome(__result, __result);
                    }
                }

                // make gear
                if (!request.Newborn)
                {
                    PawnApparelGenerator.GenerateStartingApparelFor(__result, request);
                    PawnWeaponGenerator.TryGenerateWeaponFor(__result, request);
                    PawnInventoryGenerator.GenerateInventoryFor(__result, request);
                }
            }
        }

        private static Gender GetGender(float maleChance, Gender? fixedGender)
        {
            if (fixedGender.HasValue)
            {
                if (fixedGender == Gender.Male && maleChance > 0)
                {
                    return Gender.Male;
                }
                else if (fixedGender == Gender.Female && maleChance < 1)
                {
                    return Gender.Female;
                }
                else
                {
                    Log.Warning("====================================================================================================");
                    Log.Warning("[Garam, Race Addon] The pawn's fixed gender is <" + fixedGender.ToString() + ">, but that gender cannot be created");
                    Log.Warning("====================================================================================================");
                }
            }
            if (Rand.Chance(maleChance))
            {
                return Gender.Male;
            }
            return Gender.Female;
        }
        internal static void GenerateAppearance(Pawn target, AppearanceDef def)
        {
            var racomp = target.GetComp<RaceAddonComp>();
            var thingDef = (target.def as RaceAddonThingDef);
            // make color palette list
            List<Pair<string, Color>> palettes = GetColorPalletes(def.colorPalettes);

            // make skin
            if (def.skinList.Count > 0)
            {
                target.story.melanin = 0f;
                var skinSet = def.skinList.RandomElementByWeight((AppearanceDef.SkinSet x) => x.weight);
                racomp.savedSkinData = new SavedSkinData
                {
                    color1 = palettes.First(x => x.First == skinSet.skinColor.color1_PaletteName).Second,
                    color2 = palettes.First(x => x.First == skinSet.skinColor.color2_PaletteName).Second
                };
                if (thingDef.raceAddonSettings.graphicSetting.rottingColor != null)
                {
                    racomp.savedSkinData.rottingColor = thingDef.raceAddonSettings.graphicSetting.rottingColor.NewRandomizedColor();
                }
                else
                {
                    racomp.savedSkinData.rottingColor = new Color(0.34f, 0.32f, 0.3f);
                }
            }

            // make body
            if (def.bodyList.Count > 0)
            {
                var bodySet = def.bodyList.RandomElementByWeight((AppearanceDef.BodySet x) => x.weight);
                racomp.savedBodyData = new SavedBodyData
                {
                    def = bodySet.bodyDef
                };
                target.story.bodyType = bodySet.bodyDef.bodyTypeDef;
            }

            // make head
            if (def.bodyList.Count > 0)
            {
                var headSet = def.headList.RandomElementByWeight((AppearanceDef.HeadSet x) => x.weight);
                racomp.savedHeadData = new SavedHeadData
                {
                    def = headSet.headDef
                };
                Traverse.Create(target.story).Field("headGraphicPath").SetValue(headSet.headDef.replacedHeadPath);
                target.story.crownType = headSet.headDef.crownType;
            }

            // make face
            if (def.faceList.Count > 0)
            {
                var faceSet = def.faceList.RandomElementByWeight((AppearanceDef.FaceSet x) => x.weight);
                racomp.savedFaceData = new SavedFaceData
                {
                    color1 = palettes.First(x => x.First == faceSet.faceColor.color1_PaletteName).Second,
                    color2 = palettes.First(x => x.First == faceSet.faceColor.color2_PaletteName).Second
                };
                if (faceSet.upperFaceDef != null)
                {
                    racomp.savedFaceData.upperDef = faceSet.upperFaceDef;
                }
                if (faceSet.lowerFaceDef != null)
                {
                    racomp.savedFaceData.lowerDef = faceSet.lowerFaceDef;
                }
            }

            // make hair
            racomp.savedHairData = new SavedHairData();
            if (def.hairList.Count > 0)
            {
                var hairSet = def.hairList.RandomElementByWeight((AppearanceDef.HairSet x) => x.weight);
                List<string> hairTags = hairSet.hairTags;
                if (hairTags != null && hairTags.Count > 0)
                {
                    List<HairDef> hairs = DefDatabase<HairDef>.AllDefsListForReading.FindAll((HairDef x) => x.hairTags.SharesElementWith(hairTags));
                    target.story.hairDef = hairs.RandomElementByWeight((HairDef x) =>
                    Traverse.Create(typeof(PawnHairChooser)).Method("HairChoiceLikelihoodFor", new[] { typeof(HairDef), typeof(Pawn) }).GetValue<float>(x, target));
                }
                target.story.hairColor = palettes.First(x => x.First == hairSet.hairColor.color1_PaletteName).Second;
                racomp.savedHairData.color2 = palettes.First(x => x.First == hairSet.hairColor.color2_PaletteName).Second;
            }
            if (thingDef.raceAddonSettings.healthSetting.greyHairAt != 0)
            {
                int greyHairAt = thingDef.raceAddonSettings.healthSetting.greyHairAt;
                if (target.ageTracker.AgeBiologicalYears > greyHairAt)
                {
                    float num = GenMath.SmootherStep(greyHairAt, greyHairAt * 2, target.ageTracker.AgeBiologicalYears);
                    if (Rand.Value < num)
                    {
                        float num2 = Rand.Range(0.65f, 0.85f);
                        target.story.hairColor = new Color(num2, num2, num2);
                    }
                }
            }

            // make addons
            if (def.addonList.Count > 0)
            {
                racomp.savedAddonDatas = new List<SavedAddonData>();
                foreach (AppearanceDef.AddonSet addonSet in def.addonList)
                {
                    string path = addonSet.addonDef.texturePath;
                    if (addonSet.addonDef.linkedBodyPart == "None")
                    {
                        foreach (var hediff in target.health.hediffSet.hediffs.FindAll(x => x.Part == null))
                        {
                            if (addonSet.addonDef.hediffPaths.Find(x => x.hediffDef == hediff.def) is var info && info != null)
                            {
                                path = info.path;
                            }
                        }
                    }
                    else
                    {
                        foreach (var hediff in target.health.hediffSet.hediffs.FindAll(x => x.Part.untranslatedCustomLabel == addonSet.addonDef.linkedBodyPart))
                        {
                            if (addonSet.addonDef.hediffPaths.Find(x => x.hediffDef == hediff.def) is var info && info != null)
                            {
                                path = info.path;
                            }
                        }
                    }

                    SavedAddonData data = new SavedAddonData
                    {
                        def = addonSet.addonDef,
                        color1 = palettes.First(x => x.First == addonSet.addonColor.color1_PaletteName).Second,
                        color2 = palettes.First(x => x.First == addonSet.addonColor.color2_PaletteName).Second,
                        texturePath = path
                    };
                    racomp.savedAddonDatas.Add(data);
                }
            }
        }
        private static List<Pair<string, Color>> GetColorPalletes(List<AppearanceDef.ColorPalette> list)
        {
            List<Pair<string, Color>> result = new List<Pair<string, Color>>();
            foreach (var set in list)
            {
                if (set.color != null)
                {
                    result.Add(new Pair<string, Color>(set.paletteName, set.color.NewRandomizedColor()));
                }
                else if (set.melanin.max + set.melanin.min >= 0)
                {
                    result.Add(new Pair<string, Color>(set.paletteName, PawnSkinColors.GetSkinColor(Rand.Range(set.melanin.min, set.melanin.max))));
                }
                else
                {
                    result.Add(new Pair<string, Color>(set.paletteName, RandomHairColor()));
                }
            }
            return result;
        }

        private static Color RandomHairColor()
        {
            if (Rand.Value < 0.02f)
            {
                return new Color(Rand.Value, Rand.Value, Rand.Value);
            }
            if (Rand.Value < 0.5f)
            {
                float value = Rand.Value;
                if (value < 0.25f)
                {
                    return new Color(0.2f, 0.2f, 0.2f);
                }
                if (value < 0.5f)
                {
                    return new Color(0.31f, 0.28f, 0.26f);
                }
                if (value < 0.75f)
                {
                    return new Color(0.25f, 0.2f, 0.15f);
                }
                return new Color(0.3f, 0.2f, 0.1f);
            }
            else
            {
                float value2 = Rand.Value;
                if (value2 < 0.25f)
                {
                    return new Color(0.353f, 0.228f, 0.126f);
                }
                if (value2 < 0.5f)
                {
                    return new Color(0.518f, 0.326f, 0.184f);
                }
                if (value2 < 0.75f)
                {
                    return new Color(0.757f, 0.573f, 0.333f);
                }
                return new Color(0.930f, 0.800f, 0.612f);
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateGearFor")]
    public static class HarmonyPatches_GenerateGearFor
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef)
            {
                return false;
            }
            return true;
        }
    }
}
