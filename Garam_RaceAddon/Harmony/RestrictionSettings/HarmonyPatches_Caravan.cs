using RimWorld;
using Verse;
using HarmonyLib;
using RimWorld.Planet;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(WITab_Caravan_Gear))]
    [HarmonyPatch("TryEquipDraggedItem")]
    public static class HarmonyPatches_TryEquipDraggedItem
    {
        [HarmonyPrefix]
        private static bool Prefix(Pawn p, ref Thing ___draggedItem, ref bool ___droppedDraggedItem)
        {
            ___droppedDraggedItem = false;
            if (___draggedItem.def.IsApparel && !RaceAddonTools.CheckApparel(p, ___draggedItem.def))
            {
                Messages.Message("RaceAddonRestriction_Caravan".Translate(p.LabelShort), p, MessageTypeDefOf.RejectInput, false);
                ___draggedItem = null;
                return false;
            }
            if (___draggedItem.def.IsWeapon && !RaceAddonTools.CheckWeapon(p, ___draggedItem.def))
            {
                Messages.Message("RaceAddonRestriction_Caravan".Translate(p.LabelShort), p, MessageTypeDefOf.RejectInput, false);
                ___draggedItem = null;
                return false;
            }
            return true;
        }
    }
}
