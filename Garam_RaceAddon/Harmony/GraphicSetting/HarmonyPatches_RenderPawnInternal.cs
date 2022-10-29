using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using HarmonyLib;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    public static class HarmonyPatches_RenderPawnInternal
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        private static bool Prefix(PawnRenderer __instance, Pawn ___pawn, PawnWoundDrawer ___woundOverlays, PawnHeadOverlays ___statusOverlays,
            Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible)
        {
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                //====================================================================================================
                if (!__instance.graphics.AllResolved)
                {
                    __instance.graphics.ResolveAllGraphics();
                }
                //====================================================================================================
                RaceAddonComp racomp = ___pawn.GetComp<RaceAddonComp>();
                //====================================================================================================
                Quaternion bodyQuat = Quaternion.AngleAxis(angle, Vector3.up);
                //====================================================================================================
                Quaternion headQuat = bodyQuat;
				var flag1 = false;
                if (!portrait && ___pawn.Awake())
                {
                    if (racomp.headRotator != null && !___pawn.Drafted)
                    {
                        headQuat *= racomp.headRotator.GetQuat();
                    }
                    if (racomp.headTargeter != null && !___pawn.Downed)
                    {
						var initialRot = headFacing;
						headFacing.Rotate(racomp.headTargeter.RotDirection);
						if (initialRot != headFacing)
						{
							flag1 = true;
						}
                    }
                }
                //====================================================================================================
                Mesh bodyMesh = racomp.bodyMeshSet.MeshAt(bodyFacing);
                //====================================================================================================
                Mesh headMesh = racomp.headMeshSet.MeshAt(headFacing);
                //====================================================================================================
                Vector3 bodyLoc = rootLoc;
				//bodyLoc.y += 0.022482f;
				bodyLoc.y += 0.022482f;
				//====================================================================================================
				Vector3 headLoc = __instance.BaseHeadOffsetAt(bodyFacing);
                if (portrait || renderBody)
                {
                    headLoc.x *= thingDef.raceAddonSettings.graphicSetting.drawSize[racomp.drawSize].headSize.x;
                    headLoc.z *= thingDef.raceAddonSettings.graphicSetting.drawSize[racomp.drawSize].headSize.y;
				}
				if (!portrait && flag1)
				{
					if (headFacing == Rot4.South)
					{
						headLoc += racomp.savedHeadData.def.headTargetingOffsets.south;
					}
					else if (headFacing == Rot4.East)
					{
						headLoc += racomp.savedHeadData.def.headTargetingOffsets.east;
					}
					else if (headFacing == Rot4.West)
					{
						headLoc += racomp.savedHeadData.def.headTargetingOffsets.west;
					}
					else
					{
						headLoc += racomp.savedHeadData.def.headTargetingOffsets.north;
					}
				}
				headLoc = bodyLoc + (bodyQuat * headLoc);
				//====================================================================================================
				List<int> shell = new List<int>();
				List<int> hat = new List<int>();
				List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
				for (int i = 0; i < apparelGraphics.Count; i++)
				{
					if (apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
					{
						shell.Add(i);
					}
					if (apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
					{
						hat.Add(i);
					}
				}
				//====================================================================================================
				Vector3 drawLoc = new Vector3();
				if (renderBody)
				{
					//Draw Body, Apparel
					drawLoc = DrawLoc(bodyLoc, 0.0002f);
					List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
					for (int i = 0; i < list.Count; i++)
					{
						Material mat = OverrideMaterialIfNeeded(list[i], ___pawn, __instance.graphics);
						GenDraw.DrawMeshNowOrLater(bodyMesh, drawLoc, bodyQuat, mat, portrait);
						drawLoc.y += 0.0001f;
					}
				//Draw Shell
					if (shell.Count() > 0)
					{
						drawLoc = DrawLoc(bodyLoc, 0.0009f);
						foreach (int i in shell)
						{
							Material mat = apparelGraphics[i].graphic.MatAt(bodyFacing);
							mat = OverrideMaterialIfNeeded(mat, ___pawn, __instance.graphics);
							GenDraw.DrawMeshNowOrLater(bodyMesh, drawLoc, bodyQuat, mat, portrait);
						}
					}
				//Draw Wound
					drawLoc = DrawLoc(bodyLoc, 0.0020f);
					if (bodyDrawType == RotDrawMode.Fresh)
					{
						___woundOverlays.RenderOverBody(drawLoc, bodyMesh, bodyQuat, portrait);
					}
				//Draw Body Addons
					if (racomp.bodyAddonGraphicSets != null)
					{
						foreach (var set in racomp.bodyAddonGraphicSets)
						{
							if (set.draw)
							{
								drawLoc = bodyLoc;
								ResolveAddonLoc(ref drawLoc, set.data.def, bodyFacing);
								Material mat = set.MatAt(bodyFacing, bodyDrawType);
								if (mat != null)
								{
									mat = OverrideMaterialIfNeeded(mat, ___pawn, __instance.graphics);
									GenDraw.DrawMeshNowOrLater(bodyMesh, drawLoc, bodyQuat, mat, portrait);
								}
							}
						}
					}
				}
				if (__instance.graphics.headGraphic != null)
				{
				//Draw Head
					Material headMat = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
					if (headMat != null)
					{
						drawLoc = DrawLoc(headLoc, 0.0011f);
						GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, headMat, portrait);
					}
				//Draw Hat or Mask
					bool flag2 = false;
					if (!portrait || !Prefs.HatsOnlyOnMap)
					{
						if (hat.Count() > 0 && racomp.drawHat)
						{
							foreach (int i in hat)
							{
								if (apparelGraphics[i].sourceApparel.def.apparel.hatRenderedFrontOfFace) //Mask
								{
									flag2 = true;
									drawLoc = headFacing == Rot4.North ? DrawLoc(headLoc, -0.0001f) : DrawLoc(headLoc, 0.0017f);
								}
								else
								{
									drawLoc = DrawLoc(headLoc, 0.0015f); //Hat
								}
								Material mat = apparelGraphics[i].graphic.MatAt(headFacing);
								mat = OverrideMaterialIfNeeded(mat, ___pawn, __instance.graphics);
								GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, mat, portrait);
							}
						}
					}
					if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
					{
					//Draw Face
						if (racomp.upperFaceGraphicSet != null)
						{
							drawLoc = DrawLoc(headLoc, 0.0014f);
							Material mat = OverrideMaterialIfNeeded(racomp.upperFaceGraphicSet.MatAt(headFacing, portrait), ___pawn, __instance.graphics);
							GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, mat, portrait);
						}
						if (racomp.lowerFaceGraphicSet != null)
						{
							drawLoc = DrawLoc(headLoc, 0.0012f);
							Material mat = OverrideMaterialIfNeeded(racomp.lowerFaceGraphicSet.MatAt(headFacing, portrait), ___pawn, __instance.graphics);
							GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, mat, portrait);
						}
					//Draw Hair
						if (flag2 || racomp.drawUpperHair)
						{
							drawLoc = DrawLoc(headLoc, 0.0013f);
							Material mat = __instance.graphics.HairMatAt(headFacing);
							mat = OverrideMaterialIfNeeded(mat, ___pawn, __instance.graphics);
							GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, mat, portrait);
						}
						if (racomp.improvedHairGraphic != null)
						{
							if (flag2 || racomp.drawLowerHair)
							{
								if (___pawn.InBed() && ((___pawn.story.hairDef as ImprovedHairDef).drawnInBed) || renderBody)
								{
									drawLoc = DrawLoc(headLoc, 0.0001f);
									Material mat = racomp.improvedHairGraphic.MatAt(headFacing);
									mat = OverrideMaterialIfNeeded(mat, ___pawn, __instance.graphics);
									GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, mat, portrait);
								}
							}
						}
					//Draw Head Addons
						if (racomp.headAddonGraphicSets != null)
						{
							foreach (var set in racomp.headAddonGraphicSets)
							{
								if (set.draw)
								{
									drawLoc = headLoc;
									ResolveAddonLoc(ref drawLoc, set.data.def, headFacing);
									Material mat = set.MatAt(headFacing, bodyDrawType);
									if (mat != null)
									{
										mat = OverrideMaterialIfNeeded(mat, ___pawn, __instance.graphics);
										GenDraw.DrawMeshNowOrLater(headMesh, drawLoc, headQuat, mat, portrait);
									}
								}
							}
						}
					}
				}
				if (!portrait)
				{
					drawLoc = DrawLoc(bodyLoc, 0.01f);
					DrawEquipment(___pawn, racomp.equipmentMeshSet.MeshAt(bodyFacing), drawLoc);
				//Draw Apparel Extras
					if (___pawn.apparel != null)
					{
						List<Apparel> wornApparel = ___pawn.apparel.WornApparel;
						for (int l = 0; l < wornApparel.Count; l++)
						{
							wornApparel[l].DrawWornExtras();
						}
					}
				//Draw Overlays
					drawLoc = DrawLoc(bodyLoc, 0.02f);
					___statusOverlays.RenderStatusOverlays(drawLoc, bodyQuat, headMesh);
				}
				return false;
            }
            return true;
        }
        private static Material OverrideMaterialIfNeeded(Material original, Pawn pawn, PawnGraphicSet graphics)
        {
            Material baseMat = pawn.IsInvisible() ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return graphics.flasher.GetDamagedMat(baseMat);
        }
		private static Vector3 DrawLoc(Vector3 loc, float y)
		{
			loc.y += (y * 2f);
			//it was annoying to fix everything
			return loc;
		}
		private static void ResolveAddonLoc(ref Vector3 loc, AddonDef def, Rot4 rot)
		{
			if (rot == Rot4.South)
			{
				if (def.inFrontOfBody.south)
				{
					loc.y += 0.0032f;
				}
				loc += def.offsets.south;
			}
			else if (rot == Rot4.East)
			{
				if (def.inFrontOfBody.east)
				{
					loc.y += 0.0032f;
				}
				loc += def.offsets.east;
			}
			else if (rot == Rot4.West)
			{
				if (def.inFrontOfBody.west)
				{
					loc.y += 0.0032f;
				}
				loc += def.offsets.west;
			}
			else
			{
				if (def.inFrontOfBody.north)
				{
					loc.y += 0.0032f;
				}
				loc += def.offsets.north;
			}
		}
		private static void DrawEquipment(Pawn pawn, Mesh mesh, Vector3 loc)
		{
			if (pawn.Dead || !pawn.Spawned || pawn.equipment == null || pawn.equipment.Primary == null || (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon))
			{
				return;
			}
			if (pawn.stances.curStance is Stance_Busy stance_Busy && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
			{
				Vector3 a;
				if (stance_Busy.focusTarg.HasThing)
				{
					a = stance_Busy.focusTarg.Thing.DrawPos;
				}
				else
				{
					a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
				}
				float num = 0f;
				if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
				{
					num = (a - pawn.DrawPos).AngleFlat();
				}
				Vector3 aimLoc = loc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
				DrawEquipmentAiming(pawn.equipment.Primary, num, mesh, aimLoc);
			}
			else if (CarryWeaponOpenly(pawn))
			{

				if (pawn.Rotation == Rot4.South)
				{
					Vector3 aimLoc2 = loc + new Vector3(0f, 0f, -0.22f);
					DrawEquipmentAiming(pawn.equipment.Primary, 143f, mesh, aimLoc2);
				}
				else if (pawn.Rotation == Rot4.North)
				{
					Vector3 aimLoc3 = loc + new Vector3(0f, -0.012f, -0.11f);
					DrawEquipmentAiming(pawn.equipment.Primary, 143f, mesh, aimLoc3);
				}
				else if (pawn.Rotation == Rot4.East)
				{
					Vector3 aimLoc4 = loc + new Vector3(0.2f, 0f, -0.22f);
					DrawEquipmentAiming(pawn.equipment.Primary, 143f, mesh, aimLoc4);
				}
				else if (pawn.Rotation == Rot4.West)
				{
					Vector3 aimLoc5 = loc + new Vector3(-0.2f, 0f, -0.22f);
					DrawEquipmentAiming(pawn.equipment.Primary, 217f, mesh, aimLoc5);
				}
			}
		}
		private static void DrawEquipmentAiming(Thing weapon, float aimAngle, Mesh mesh, Vector3 loc)
		{
			float num = aimAngle - 90f;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				num += weapon.def.equippedAngleOffset;
			}
			else if (aimAngle > 200f && aimAngle < 340f)
			{
				num -= 180f;
				num -= weapon.def.equippedAngleOffset;
			}
			else
			{
				num += weapon.def.equippedAngleOffset;
			}
			num %= 360f;
			Material matSingle;
			if (weapon.Graphic is Graphic_StackCount graphic_StackCount)
			{
				matSingle = graphic_StackCount.SubGraphicForStackCount(1, weapon.def).MatSingle;
			}
			else
			{
				matSingle = weapon.Graphic.MatSingle;
			}
			Graphics.DrawMesh(mesh, loc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
		}
		private static bool CarryWeaponOpenly(Pawn pawn)
		{
			return (pawn.carryTracker == null || pawn.carryTracker.CarriedThing == null) && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
		}
	}
}