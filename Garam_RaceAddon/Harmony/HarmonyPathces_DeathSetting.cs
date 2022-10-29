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
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("ShouldBeDead")]
    public static class HarmonyPatches_InstantDeath
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn ___pawn, ref bool __result)
        {
            if (!__result && ___pawn.def is RaceAddonThingDef thingDef)
            {
                DeathSetting.ImmediateDeath.FactionCheck set;
                if (___pawn.Faction == null)
                {
                    set = thingDef.raceAddonSettings.deathSetting.immediateDeath.noneFaction;
                }
                else if (___pawn.Faction.IsPlayer)
                {
                    set = thingDef.raceAddonSettings.deathSetting.immediateDeath.playerFaction;
                }
                else if (!___pawn.Faction.HostileTo(Faction.OfPlayer))
                {
                    set = thingDef.raceAddonSettings.deathSetting.immediateDeath.friendlyFaction;
                }
                else
                {
                    set = thingDef.raceAddonSettings.deathSetting.immediateDeath.hostileFaction;
                }

                if (set.inPainShock && ___pawn.health.InPainShock)
                {
                    __result = true;
                }
                else if (set.unconscious && ___pawn.health.capacities.CanBeAwake)
                {
                    __result = true;
                }
                else if (set.immovable && ___pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                {
                    __result = true;
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    public static class HarmonyPatches_Kill
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn __instance)
        {
            if (__instance.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.deathSetting.leavings.items != null && __instance.Spawned)
                {
                    foreach (var bonus in thingDef.raceAddonSettings.deathSetting.leavings.items)
                    {
                        if (Rand.Chance(bonus.chance))
                        {
                            Thing item = ThingMaker.MakeThing(bonus.thingDef, null);
                            item.stackCount = bonus.useMeatAmount ? (int)__instance.GetStatValue(StatDefOf.MeatAmount, true) : Rand.Range(bonus.stackCount.min, bonus.stackCount.max);
                            if (item.stackCount > 0)
                            {
                                GenPlace.TryPlaceThing(item, __instance.Position, __instance.Map, ThingPlaceMode.Near);
                            }
                        }
                    }
                }
            }
            return true;
        }
        
        [HarmonyPriority(int.MinValue)]
        [HarmonyPostfix]
        private static void Postfix(Pawn __instance)
        {
            if (__instance.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.deathSetting.leavings.corpse)
                {
                    if (!thingDef.raceAddonSettings.deathSetting.leavings.equipment)
                    {
                        __instance.equipment.DestroyAllEquipment();
                    }
                    if (!thingDef.raceAddonSettings.deathSetting.leavings.apparel)
                    {
                        __instance.apparel.DestroyAll();
                    }
                }
                else
                {
                    if (thingDef.raceAddonSettings.deathSetting.leavings.equipment)
                    {
                        __instance.equipment.DropAllEquipment(__instance.Position);
                    }
                    if (thingDef.raceAddonSettings.deathSetting.leavings.apparel)
                    {
                        __instance.apparel.DropAll(__instance.Position);
                    }
                    __instance.Corpse.Destroy();
                }
            }
        }
    }
}