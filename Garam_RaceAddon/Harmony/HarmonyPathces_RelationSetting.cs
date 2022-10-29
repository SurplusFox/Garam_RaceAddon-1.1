using RimWorld;
using Verse;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawnRelations")]
    public static class HarmonyPatches_GeneratePawnRelations
    {
        private static readonly PawnRelationDef[] blood = DefDatabase<PawnRelationDef>.AllDefsListForReading.FindAll
            ((PawnRelationDef x) => x.familyByBloodRelation && x.generationChanceFactor > 0f).ToArray();
        private static readonly PawnRelationDef[] nonBlood = DefDatabase<PawnRelationDef>.AllDefsListForReading.FindAll
            ((PawnRelationDef x) => !x.familyByBloodRelation && x.generationChanceFactor > 0f).ToArray();
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                if (!thingDef.raceAddonSettings.relationSetting.randomRelationAllow)
                {
                    return false;
                }
                Pawn[] targets = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                  where x.def == pawn.def
                                  select x).ToArray();
                if (targets.Length == 0)
                {
                    return false;
                }
                float num = 45f;
                num += targets.ToList().FindAll((Pawn x) => !x.Discarded).Count * 2.7f;

                List<Pair<Pawn, PawnRelationDef>> pairs = new List<Pair<Pawn, PawnRelationDef>>();
                foreach (Pawn target in targets)
                {
                    pairs.Add(new Pair<Pawn, PawnRelationDef>(target, blood.RandomElement()));
                }
                PawnGenerationRequest localReq = request;
                Pair<Pawn, PawnRelationDef> pair = pairs.RandomElementByWeightWithDefault
                    ((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num * 40f / (targets.Count() * blood.Count()));
                if (pair.First != null)
                {
                    pair.Second.Worker.CreateRelation(pawn, pair.First, ref request);
                }
                Pair<Pawn, PawnRelationDef> pair2 = pairs.RandomElementByWeightWithDefault
                    ((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num * 40f / (targets.Count() * nonBlood.Count()));
                if (pair2.First != null)
                {
                    pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
                }
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Pawn_RelationsTracker))]
    [HarmonyPatch("CompatibilityWith")]
    public static class HarmonyPatches_CompatibilityWith
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn_RelationsTracker __instance, Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            if (!___pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || ___pawn == otherPawn)
            {
                __result = 0f;
                return false;
            }
            float num1 = Mathf.Abs(___pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num2 = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, num1);
            num2 = Mathf.Clamp(num2, -0.45f, 0.45f);
            float num3 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);

            float num4 = 0f;

            if (___pawn.def is RaceAddonThingDef thingDef1)
            {
                if (thingDef1.raceAddonSettings.relationSetting.specialRaces.Find(x => x.raceDef == otherPawn.def) is var set && set != null)
                {
                    num4 = set.compatibilityBonus;
                }
                else if (___pawn.def == otherPawn.def || thingDef1.raceAddonSettings.relationSetting.sameRaces.Contains(otherPawn.def))
                {
                    num4 = thingDef1.raceAddonSettings.relationSetting.sameRace.compatibilityBonus;
                }
                else
                {
                    num4 = thingDef1.raceAddonSettings.relationSetting.otherRace.compatibilityBonus;
                }
            }

            if (otherPawn.def is RaceAddonThingDef thingDef2)
            {
                if (thingDef2.raceAddonSettings.relationSetting.specialRaces.Find(x => x.raceDef == otherPawn.def) is var set && set != null)
                {
                    num4 = set.compatibilityBonus;
                }
                else if (___pawn.def == otherPawn.def || thingDef2.raceAddonSettings.relationSetting.sameRaces.Contains(otherPawn.def))
                {
                    num4 = thingDef2.raceAddonSettings.relationSetting.sameRace.compatibilityBonus;
                }
                else
                {
                    num4 = thingDef2.raceAddonSettings.relationSetting.otherRace.compatibilityBonus;
                }
            }

            __result = num2 + num3 + num4;
            return false;
        }
    }
    [HarmonyPatch(typeof(Pawn_RelationsTracker))]
    [HarmonyPatch("SecondaryLovinChanceFactor")]
    public static class HarmonyPatches_SecondaryLovinChanceFactor
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            Pawn pawn = ___pawn;
            if (!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || ___pawn == otherPawn)
            {
                __result = 0f;
                return false;
            }
            if (pawn.story != null && pawn.story.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual)) //무성애 = 0f
                {
                    __result = 0f;
                    return false;
                }
                if (!pawn.story.traits.HasTrait(TraitDefOf.Bisexual)) //!양성애
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender != pawn.gender) //동성애 && 이성 = 0f
                        {
                            __result = 0f;
                            return false;
                        }
                    }
                    else if (otherPawn.gender == pawn.gender) //이성애 && 동성 = 0f
                    {
                        __result = 0f;
                        return false;
                    }
                }
            }
            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if (ageBiologicalYearsFloat < 16f || ageBiologicalYearsFloat2 < 16f)
            {
                __result = 0f;
                return false;
            }
            float defaultValue = 1f;
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                RelationSetting.LoveChanceFactor factor = null;
                if (thingDef.raceAddonSettings.relationSetting.specialRaces.Find(x => x.raceDef == otherPawn.def) is var set && set != null) // 특별한 종족
                {
                    if (otherPawn.gender == Gender.Male) // 특별한 종족 - 남성
                    {
                        factor = set.loveChanceFactor_Male;
                    }
                    else if(otherPawn.gender == Gender.Female) // 특별한 종족 - 여성
                    {
                        factor = set.loveChanceFactor_Female;
                    }
                }
                else if (pawn.def == otherPawn.def || thingDef.raceAddonSettings.relationSetting.sameRaces.Contains(otherPawn.def)) // 같은 종족
                {
                    if (otherPawn.gender == Gender.Male) // 같은 종족 - 남성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.sameRace.loveChanceFactor_Male;
                    }
                    else if (otherPawn.gender == Gender.Female) // 같은 종족 - 여성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.sameRace.loveChanceFactor_Female;
                    }
                }
                else // 다른 종족
                {
                    if (otherPawn.gender == Gender.Male) // 다른 종족 - 남성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.otherRace.loveChanceFactor_Male;
                    }
                    else if (otherPawn.gender == Gender.Female) // 다른 종족 - 여성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.otherRace.loveChanceFactor_Female;
                    }
                }
                if (factor != null) // 계산 시작
                {
                    float 최소나이값 = factor.minAgeValue;
                    float 상대최소나이 = ageBiologicalYearsFloat - factor.minAgeDifference;
                    float 상대적은나이 = ageBiologicalYearsFloat - factor.lowerAgeDifference;
                    float 상대많은나이 = ageBiologicalYearsFloat + factor.upperAgeDifference;
                    float 상대최대나이 = ageBiologicalYearsFloat + factor.maxAgeDifference;
                    float 최대나이값 = factor.maxAgeValue;
                    defaultValue = 평탄한꼭대기(최소나이값, 상대최소나이, 상대적은나이, 상대많은나이, 상대최대나이, 최대나이값, ageBiologicalYearsFloat2);
                }
            }
            else // 바닐라
            {
                if (pawn.gender == Gender.Male)
                {
                    float min = ageBiologicalYearsFloat - 30f;
                    float lower = ageBiologicalYearsFloat - 10f;
                    float upper = ageBiologicalYearsFloat + 3f;
                    float max = ageBiologicalYearsFloat + 10f;
                    defaultValue = GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, ageBiologicalYearsFloat2);
                }
                else if (pawn.gender == Gender.Female)
                {
                    float min2 = ageBiologicalYearsFloat - 10f;
                    float lower2 = ageBiologicalYearsFloat - 3f;
                    float upper2 = ageBiologicalYearsFloat + 10f;
                    float max2 = ageBiologicalYearsFloat + 30f;
                    defaultValue = GenMath.FlatHill(0.2f, min2, lower2, upper2, max2, 0.2f, ageBiologicalYearsFloat2);
                }
            }
            float ageFactor_Pawn = Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat);
            float ageFactor_OtherPawn = Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat2);
            float beauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty, true);
            float beautyFactor = 1f;
            if (beauty < 0f)
            {
                beautyFactor = 0.3f;
            }
            else if (beauty > 0f)
            {
                beautyFactor = 2.3f;
            }
            __result = defaultValue * ageFactor_Pawn * ageFactor_OtherPawn * beautyFactor;
            return false;
        }
        private static float 평탄한꼭대기(float 최소나이값, float 상대최소나이, float 상대적은나이, float 상대많은나이, float 상대최대나이, float 최대나이값, float 상대나이)
        {
            if (상대나이 < 상대최소나이)
            {
                return 최소나이값;
            }
            if (상대나이 < 상대적은나이)
            {
                return GenMath.LerpDouble(상대최소나이, 상대적은나이, 최소나이값, 1f, 상대나이);
            }
            if (상대나이 < 상대많은나이)
            {
                return 1f;
            }
            if (상대나이 < 상대최대나이)
            {
                return GenMath.LerpDouble(상대많은나이, 상대최대나이, 1f, 최대나이값, 상대나이);
            }
            return 최대나이값;
        }
    }
    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("GetFather")]
    public static class HarmonyPatches_GetFather
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, ref Pawn __result)
        {
            if (pawn.RaceProps.IsFlesh && __result == null)
            {
                foreach (var relation in pawn.relations.DirectRelations)
                {
                    if (relation.def == PawnRelationDefOf.Parent && relation.otherPawn != pawn.GetMother())
                    {
                        __result = relation.otherPawn;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("GetMother")]
    public static class HarmonyPatches_GetMother
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn pawn, ref Pawn __result)
        {
            if (pawn.RaceProps.IsFlesh && __result == null)
            {
                foreach (var relation in pawn.relations.DirectRelations)
                {
                    if (relation.def == PawnRelationDefOf.Parent && relation.otherPawn != pawn.GetFather())
                    {
                        __result = relation.otherPawn;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("SetFather")]
    public static class HarmonyPatches_SetFather
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, Pawn newFather)
        {
            Pawn father = pawn.GetFather();
            if (father != newFather)
            {
                if (father != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, father);
                }
                if (newFather != null)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newFather);
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("SetMother")]
    public static class HarmonyPatches_SetMother
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn pawn, Pawn newMother)
        {
            Pawn mother = pawn.GetMother();
            if (mother != newMother)
            {
                if (mother != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, mother);
                }
                if (newMother != null)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newMother);
                }
            }
            return false;
        }
    }
}