using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public static class HarmonyPathces_DrawCharacterCard
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator il)
        {
            var type1 = AccessTools.FirstInner(typeof(CharacterCardUtility), x => x.Name.Contains("14_1"));
            var type2 = AccessTools.FirstInner(typeof(CharacterCardUtility), x => x.Name.Contains("14_0"));
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Call && code.operand.ToString().Contains("Font"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1); // sectionRect
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0); // num12
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(type1, "leftRect")); // leftRect
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type1, "CS$<>8__locals1"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type2, "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPathces_DrawCharacterCard), "GetRaceStory"));
                }
            }
        }

        private static void GetRaceStory(ref Rect sectionRect, ref float num12, ref Rect leftRect, Pawn pawn)
        {
            Rect rect13 = new Rect(sectionRect.x, num12, leftRect.width, 22f);
            if (Mouse.IsOver(rect13))
            {
                Widgets.DrawHighlight(rect13);
            }
            if (Mouse.IsOver(rect13))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(pawn.def.description.Translate());
                if (pawn.def is RaceAddonThingDef thingDef)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    if (thingDef.raceAddonSettings.workSetting.skillGains != null)
                    {
                        List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                        for (int i = 0; i < allDefsListForReading.Count; i++)
                        {
                            SkillDef skillDef = allDefsListForReading[i];
                            if (thingDef.raceAddonSettings.workSetting.skillGains.Find(x => x.skill == skillDef) is var skill && skill != null)
                            {
                                stringBuilder.AppendLine(skillDef.skillLabel.CapitalizeFirst() + ":   " + skill.xp.ToString("+##;-##"));
                            }
                        }
                        stringBuilder.AppendLine();
                    }
                    foreach (WorkTypeDef disabledWorkType in thingDef.DisabledWorkTypes)
                    {
                        stringBuilder.AppendLine(disabledWorkType.gerundLabel.CapitalizeFirst() + " " + "DisabledLower".Translate());
                    }
                    stringBuilder.AppendLine();
                    foreach (WorkGiverDef disabledWorkGiver in thingDef.DisabledWorkGivers)
                    {
                        stringBuilder.AppendLine(disabledWorkGiver.workType.gerundLabel.CapitalizeFirst() + ": " + disabledWorkGiver.LabelCap + " " + "DisabledLower".Translate());
                    }
                }
                string str = stringBuilder.ToString().TrimEndNewlines();
                TooltipHandler.TipRegion(rect13, Find.ActiveLanguageWorker.PostProcessed(str));
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            string str2 = "RaceAddon_Race".Translate();
            Widgets.Label(rect13, str2 + ":");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect14 = new Rect(rect13);
            rect14.x += 90f;
            rect14.width -= 90f;
            string str3 = pawn.def.label.Translate();
            Widgets.Label(rect14, str3.Truncate(rect14.width));
            num12 += rect13.height;
        }
    }
}
