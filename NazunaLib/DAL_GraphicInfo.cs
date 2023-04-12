using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace NazunaLib
{
    public class DAL_GraphicInfo : IComparer<DAL_GraphicInfo>
    {
        public DAL_GraphicInfo(Graphic g, bool b, float f, string h = null, float s = -1f, Mesh m = null)
        {
            graphic = g;
            isbaseHair = b;
            yOffSet = f;
            hairDefName = h;
            meshSize = s;
            mesh = m;
        }
        public bool isbaseHair;
        public float yOffSet;
        public Graphic graphic;
        public Mesh mesh;
        public float meshSize;
        public string hairDefName = null;

        public int Compare(DAL_GraphicInfo x, DAL_GraphicInfo y)
        {
            return x.yOffSet.CompareTo(y.yOffSet);
        }
    }
}
