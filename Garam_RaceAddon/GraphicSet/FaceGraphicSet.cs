using RimWorld;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public class FaceGraphicSet
    {
        private readonly Pawn pawn;
        private readonly SavedFaceData data;
        private readonly FaceDef def;
        private readonly RaceAddonComp racomp;
        public FaceGraphicSet(Pawn pawn, SavedFaceData data, FaceDef def, RaceAddonComp racomp)
        {
            this.pawn = pawn;
            this.data = data;
            this.def = def;
            this.racomp = racomp;
        }
        public Graphic mentalBreak;
        public Graphic aboutToBreak;
        public Graphic onEdge;
        public Graphic stressed;
        public Graphic neutral;
        public Graphic content;
        public Graphic happy;
        public Graphic sleeping;
        public Graphic painShock;
        public Graphic dead;
        public Graphic blink;
        public Graphic wink;
        public Graphic damaged;
        public Graphic drafted;
        public Graphic attacking;
        public void ResolveAllGraphics()
        {
            mentalBreak = GraphicDatabase.Get<Graphic_Multi>(def.mentalBreakPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            aboutToBreak = GraphicDatabase.Get<Graphic_Multi>(def.aboutToBreakPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            onEdge = GraphicDatabase.Get<Graphic_Multi>(def.onEdgePath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            stressed = GraphicDatabase.Get<Graphic_Multi>(def.stressedPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            neutral = GraphicDatabase.Get<Graphic_Multi>(def.neutralPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            content = GraphicDatabase.Get<Graphic_Multi>(def.contentPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            happy = GraphicDatabase.Get<Graphic_Multi>(def.happyPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            sleeping = GraphicDatabase.Get<Graphic_Multi>(def.sleepingPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            painShock = GraphicDatabase.Get<Graphic_Multi>(def.painShockPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            dead = GraphicDatabase.Get<Graphic_Multi>(def.deadPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            if ((pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.eyeBlink)
            {
                blink = GraphicDatabase.Get<Graphic_Multi>(def.blinkPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
                wink = GraphicDatabase.Get<Graphic_Multi>(def.winkPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            }
            if (def.damagedPath != null)
            {
                damaged = GraphicDatabase.Get<Graphic_Multi>(def.damagedPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            }
            if (def.draftedPath != null)
            {
                drafted = GraphicDatabase.Get<Graphic_Multi>(def.draftedPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            }
            if (def.attackingPath != null)
            {
                attacking = GraphicDatabase.Get<Graphic_Multi>(def.attackingPath, def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            }
        }
        public Material MatAt(Rot4 rot, bool portrait)
        {
            if (pawn.Dead)
            {
                return dead.MatAt(rot);
            }
            if (portrait)
            {
                return neutral.MatAt(rot);
            }
            if (pawn.health.InPainShock)
            {
                return painShock.MatAt(rot);
            }
            if (!pawn.Awake())
            {
                return sleeping.MatAt(rot);
            }
            if (attacking != null && pawn.IsFighting())
            {
                return attacking.MatAt(rot);
            }
            if (damaged != null && pawn.Drawer.renderer.graphics.flasher.FlashingNowOrRecently)
            {
                return damaged.MatAt(rot);
            }
            if (racomp.eyeBlinker != null)
            {
                if (racomp.eyeBlinker.BlinkNow)
                {
                    return blink.MatAt(rot);
                }
                if (racomp.eyeBlinker.WinkNow)
                {
                    return wink.MatAt(rot);
                }
            }
            if (drafted != null && pawn.Drafted)
            {
                return drafted.MatAt(rot);
            }
            if (pawn.MentalStateDef != null)
            {
                return mentalBreak.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true))
            {
                return aboutToBreak.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true) + 0.05f)
            {
                return onEdge.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor)
            {
                return stressed.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < 0.65f)
            {
                return neutral.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < 0.9f)
            {
                return content.MatAt(rot);
            }
            return happy.MatAt(rot);
        }
    }
}