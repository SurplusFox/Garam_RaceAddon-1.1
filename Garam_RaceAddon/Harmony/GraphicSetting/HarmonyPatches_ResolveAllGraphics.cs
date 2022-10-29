using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGraphicSet))]
    [HarmonyPatch("ResolveAllGraphics")]
    public static class HarmonyPatches_ResolveAllGraphics
    {
        [HarmonyPrefix]
        private static bool Prefix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                Pawn pawn = __instance.pawn;
                RaceAddonComp racomp = pawn.GetComp<RaceAddonComp>();
                racomp.drawSize = RaceAddonTools.GetPawnDrawSize(pawn, thingDef);
                var drawSize = thingDef.raceAddonSettings.graphicSetting.drawSize[racomp.drawSize];
                // cleaning
                __instance.ClearCache();

                // resolve mesh set
                racomp.bodyMeshSet = new GraphicMeshSet(1.5f * drawSize.bodySize.x, 1.5f * drawSize.bodySize.y);
                racomp.headMeshSet = new GraphicMeshSet(1.5f * drawSize.headSize.x, 1.5f * drawSize.headSize.y);
                racomp.equipmentMeshSet = new GraphicMeshSet(drawSize.equipmentSize.x, drawSize.equipmentSize.y);

                // resolve body
                var bodyDef = racomp.savedBodyData.def;
                __instance.pawn.story.bodyType = bodyDef.bodyTypeDef;
                __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (GetBodyNormalPath(bodyDef), bodyDef.shaderType.Shader, Vector2.one, pawn.story.SkinColor, racomp.savedSkinData.color2);
                __instance.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (GetBodyNormalPath(bodyDef), bodyDef.shaderType.Shader, Vector2.one, racomp.savedSkinData.rottingColor);
                __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(GetBodySkullPath(bodyDef), ShaderDatabase.Cutout);

                // resolve head
                var headDef = racomp.savedHeadData.def;
                __instance.pawn.story.crownType = headDef.crownType;
                __instance.headGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (headDef.replacedHeadPath, headDef.shaderType.Shader, Vector2.one, pawn.story.SkinColor, racomp.savedSkinData.color2);
                __instance.desiccatedHeadGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (headDef.replacedHeadPath, headDef.shaderType.Shader, Vector2.one, racomp.savedSkinData.rottingColor);
                __instance.skullGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (headDef.replacedSkullPath, headDef.shaderType.Shader, Vector2.one, Color.white);

                // resolve stump
                __instance.headStumpGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (headDef.replacedStumpPath, headDef.shaderType.Shader, Vector2.one, pawn.story.SkinColor, racomp.savedSkinData.color2);
                __instance.desiccatedHeadStumpGraphic = GraphicDatabase.Get<Graphic_Multi>
                    (headDef.replacedStumpPath, headDef.shaderType.Shader, Vector2.one, racomp.savedSkinData.rottingColor);

                // resolve hair
                HairDef hairDef = pawn.story.hairDef;
                if (hairDef is ImprovedHairDef advancedHairDef)
                {
                    __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>
                        (advancedHairDef.texPath, advancedHairDef.shaderType.Shader, Vector2.one, pawn.story.hairColor, racomp.savedHairData.color2);
                    if (advancedHairDef.lowerPath != null)
                    {
                        racomp.improvedHairGraphic = GraphicDatabase.Get<Graphic_Multi>
                            (advancedHairDef.lowerPath, advancedHairDef.shaderType.Shader, Vector2.one, pawn.story.hairColor, racomp.savedHairData.color2);
                    }
                }
                else
                {
                    __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>
                        (hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                    racomp.improvedHairGraphic = null;
                }

                // resolve upper face
                if (racomp.savedFaceData != null && racomp.savedFaceData.upperDef != null)
                {
                    var upperFaceDef = racomp.savedFaceData.upperDef;
                    racomp.upperFaceGraphicSet = new FaceGraphicSet(pawn, racomp.savedFaceData, upperFaceDef, racomp);
                    racomp.upperFaceGraphicSet.ResolveAllGraphics();
                }

                // resolve lower face
                if (racomp.savedFaceData != null && racomp.savedFaceData.lowerDef != null)
                {
                    var lowerFaceDef = racomp.savedFaceData.lowerDef;
                    racomp.lowerFaceGraphicSet = new FaceGraphicSet(pawn, racomp.savedFaceData, lowerFaceDef, racomp);
                    racomp.lowerFaceGraphicSet.ResolveAllGraphics();
                }

                // resolve addons
                if (racomp.savedAddonDatas != null)
                {
                    racomp.bodyAddonGraphicSets = null;
                    racomp.headAddonGraphicSets = null;
                    foreach (var data in racomp.savedAddonDatas)
                    {
                        if (data.def.drawingToBody)
                        {
                            if (racomp.bodyAddonGraphicSets == null)
                            {
                                racomp.bodyAddonGraphicSets = new List<AddonGraphicSet>();
                            }
                            var set = new AddonGraphicSet(data);
                            set.ResolveAllGraphics(racomp.savedSkinData.rottingColor, pawn.health.hediffSet);
                            racomp.bodyAddonGraphicSets.Add(set);
                        }
                        else
                        {
                            if (racomp.headAddonGraphicSets == null)
                            {
                                racomp.headAddonGraphicSets = new List<AddonGraphicSet>();
                            }
                            var set = new AddonGraphicSet(data);
                            set.ResolveAllGraphics(racomp.savedSkinData.rottingColor, pawn.health.hediffSet);
                            racomp.headAddonGraphicSets.Add(set);
                        }
                    }
                }

                if (!pawn.Dead)
                {
                    // resolve eye blinker
                    if (thingDef.raceAddonSettings.graphicSetting.eyeBlink)
                    {
                        racomp.eyeBlinker = new EyeBlinker();
                        racomp.eyeBlinker.Check(pawn.needs.mood.CurLevel);
                    }

                    // resolve head rotator
                    if (thingDef.raceAddonSettings.graphicSetting.headAnimation)
                    {
                        racomp.headRotator = new HeadRotator();
                        racomp.headRotator.Check();
                    }

                    // resolve head targeter
                    if (thingDef.raceAddonSettings.graphicSetting.headTargeting)
                    {
                        racomp.headTargeter = new HeadTargeter(__instance.pawn);
                        racomp.headTargeter.Check();
                    }
                }

                // resolve apparel
                __instance.ResolveApparelGraphics();

                return false;
            }
            return true;
        }

        private static string GetBodyNormalPath(BodyDef def)
        {
            if (def.replacedBodyPath == null)
            {
                return def.bodyTypeDef.bodyNakedGraphicPath;
            }
            else
            {
                return def.replacedBodyPath;
            }
        }
        private static string GetBodySkullPath(BodyDef def)
        {
            if (def.replacedSkullPath == null)
            {
                return def.bodyTypeDef.bodyDessicatedGraphicPath;
            }
            else
            {
                return def.replacedSkullPath;
            }
        }
    }
}