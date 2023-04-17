using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NazunaLib
{
    public class MultiRenderComp : ThingComp
    {
        /*public List<string> BehindTheBottomHair = new List<string>();
        public List<string> BottomHair = new List<string>();

        public List<string> BehindTheShell = new List<string>();
        public List<string> Shell = new List<string>();
        public List<string> FrontOfShell = new List<string>();

        public List<string> BehindTheBody = new List<string>();
        public List<string> Body = new List<string>();
        public List<string> FrontOfBody = new List<string>();

        public List<string> BehindTheApparel = new List<string>();
        public List<string> Apparel = new List<string>();
        public List<string> FrontOfApparel = new List<string>();

        public List<string> BehindTheHead = new List<string>();
        public List<string> Head = new List<string>();
        public List<string> FrontOfHead = new List<string>();

        public List<string> BehindTheMask = new List<string>();
        public List<string> Mask = new List<string>();
        public List<string> FrontOfMask = new List<string>();

        public List<string> BehindTheHair = new List<string>();
        public List<string> Hair = new List<string>();
        public List<string> FrontOfHair = new List<string>();

        public List<string> BehindTheHat = new List<string>();
        public List<string> Hat = new List<string>();
        public List<string> FrontOfHat = new List<string>();*/

        public Dictionary<string, MultiTexEpoch> storedData = new Dictionary<string, MultiTexEpoch>();

        public Dictionary<string, MultiTexEpoch> storedDataApparel = new Dictionary<string, MultiTexEpoch>();

        public List<MultiTexBatch> GetAllBatch
        {
            get
            {
                return storedData.Values.Concat(storedDataApparel.Values).SelectMany(x => x.batches).ToList();
            }
        }

        public List<Material> GetSouthSortedMats(TextureRenderLayer layer)
        {
            Rot4 s = Rot4.South;
            List<string> keys = GetAllBatch.Where(x => x.layer == layer).SelectMany(x => x.keyList).Distinct().ToList();
            keys.Sort((a, b) => ThisModData.TexLevelsDatabase[a].DrawOffsetForRot(s).y.CompareTo(ThisModData.TexLevelsDatabase[b].DrawOffsetForRot(s).y));
            List<Material> graphics = keys.Select(x => ThisModData.TexLevelsDatabase[x].Graphic.MatAt(s)).ToList();
            return graphics;
        }

        public List<Material> GetEastSortedMats(TextureRenderLayer layer)
        {
            Rot4 e = Rot4.East;
            List<string> keys = GetAllBatch.Where(x => x.layer == layer).SelectMany(x => x.keyList).Distinct().ToList();
            keys.Sort((a, b) => ThisModData.TexLevelsDatabase[a].DrawOffsetForRot(e).y.CompareTo(ThisModData.TexLevelsDatabase[b].DrawOffsetForRot(e).y));
            List<Material> graphics = keys.Select(x => ThisModData.TexLevelsDatabase[x].Graphic.MatAt(e)).ToList();
            return graphics;
        }

        public List<Material> GetNorthSortedMats(TextureRenderLayer layer)
        {
            Rot4 n = Rot4.North;
            List<string> keys = GetAllBatch.Where(x => x.layer == layer).SelectMany(x => x.keyList).Distinct().ToList();
            keys.Sort((a, b) => ThisModData.TexLevelsDatabase[a].DrawOffsetForRot(n).y.CompareTo(ThisModData.TexLevelsDatabase[b].DrawOffsetForRot(n).y));
            List<Material> graphics = keys.Select(x => ThisModData.TexLevelsDatabase[x].Graphic.MatAt(n)).ToList();
            return graphics;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            GetAllBatch.Where(x => x.layer == TextureRenderLayer.Apparel).SelectMany(x => x.keyList).Distinct().ToList().Sort((x, y) => ThisModData.TexLevelsDatabase[x].DrawOffsetForRot(Rot4.South).y.CompareTo(ThisModData.TexLevelsDatabase[y].DrawOffsetForRot(Rot4.South).y));
            Scribe_Collections.Look<string, MultiTexEpoch>(ref storedData, "storedData", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<string, MultiTexEpoch>(ref storedDataApparel, "storedDataApparel", LookMode.Value, LookMode.Deep);
            /*Scribe_Collections.Look<string>(ref BehindTheBottomHair, "BehindTheBottomHair", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref BottomHair, "BottomHair", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheShell, "BehindTheShell", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Shell, "Shell", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfShell, "FrontOfShell", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheBody, "BehindTheBody", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Body, "Body", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfBody, "FrontOfBody", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheApparel, "BehindTheApparel", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Apparel, "Apparel", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfApparel, "FrontOfApparel", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheHead, "BehindTheHead", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Head, "Head", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfHead, "FrontOfHead", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheMask, "BehindTheMask", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Mask, "Mask", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfMask, "FrontOfMask", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheHair, "BehindTheHair", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Hair, "Hair", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfHair, "FrontOfHair", LookMode.Value, Array.Empty<object>());

            Scribe_Collections.Look<string>(ref BehindTheHat, "BehindTheHat", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref Hat, "Hat", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<string>(ref FrontOfHat, "FrontOfHat", LookMode.Value, Array.Empty<object>());*/
        }
    }
}
