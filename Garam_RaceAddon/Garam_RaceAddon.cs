using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Garam_RaceAddon
{
    [StaticConstructorOnStartup]
    public static class Garam_RaceAddon
    {
        static Garam_RaceAddon()
        {
            var harmony = new Harmony("com.rimworld.Dalrae.Garam_RaceAddon");
            harmony.PatchAll();
            harmony.Patch(AccessTools.FirstMethod(AccessTools.FirstInner(typeof(CharacterCardUtility), x => x.Name.Contains("14_1")), x => x.Name.Contains("b__23")), null, null, new HarmonyMethod(typeof(HarmonyPathces_DrawCharacterCard), "Transpiler"));

            //==================== Recipe Import ====================//
            foreach (var thingDef in DefDatabase<RaceAddonThingDef>.AllDefs)
            {
                if (thingDef.raceAddonSettings.basicSetting.humanRecipeImport)
                {
                    ThingDef human = DefDatabase<ThingDef>.AllDefs.First((ThingDef def) => def.defName == "Human");
                    foreach (RecipeDef recipe in human.AllRecipes)
                    {
                        if (!recipe.targetsBodyPart ||
                            recipe.appliedOnFixedBodyParts.NullOrEmpty() ||
                            recipe.appliedOnFixedBodyParts.Any((BodyPartDef def) => thingDef.race.body.AllParts.Any((BodyPartRecord bpr) => bpr.def == def)))
                        {
                            if (!thingDef.recipes.Contains(recipe))
                            {
                                thingDef.recipes.Add(recipe);
                            }
                        }
                    }
                }
            }
        }
    }
}