using System.Linq;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public class AddonGraphicSet
    {
        internal SavedAddonData data;
        public AddonGraphicSet(SavedAddonData data)
        {
            this.data = data;
        }
        public Graphic defaultGraphic;
        public Graphic rottingGraphic;
        public bool draw = true;

        public void ResolveAllGraphics(Color rottingColor, HediffSet hediffSet)
        {
            string path = data.texturePath;
            if (data.def.linkedBodyPart == "None")
            {
                foreach (var hediff in hediffSet.hediffs.FindAll(x => x.Part == null))
                {
                    if (data.def.hediffPaths.Find(x => x.hediffDef == hediff.def) is var info && info != null)
                    {
                        path = info.path;
                    }
                }
            }
            else
            {
                foreach (var hediff in hediffSet.hediffs.FindAll(x => x.Part != null && x.Part.untranslatedCustomLabel == data.def.linkedBodyPart))
                {
                    if (data.def.hediffPaths.Find(x => x.hediffDef == hediff.def) is var info && info != null)
                    {
                        path = info.path;
                    }
                }
            }
            defaultGraphic = GraphicDatabase.Get<Graphic_Multi>(path, data.def.shaderType.Shader, Vector2.one, data.color1, data.color2);
            rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(path, data.def.shaderType.Shader, Vector2.one, rottingColor);
        }
        public Material MatAt(Rot4 rot, RotDrawMode rotting)
        {
            if (rotting == RotDrawMode.Fresh)
            {
                return defaultGraphic.MatAt(rot);
            }
            else if (rotting == RotDrawMode.Rotting)
            {
                if (!data.def.rotting)
                {
                    return defaultGraphic.MatAt(rot);
                }
                return rottingGraphic.MatAt(rot);
            }
            return null;
        }
    }
}