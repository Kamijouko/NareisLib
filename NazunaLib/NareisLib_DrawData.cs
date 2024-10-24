using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class NareisLib_DrawData : DrawData
    {
        public float southLayer;
        public float eastLayer;
        public float westLayer;
        public float northLayer;


        public DrawData.RotationalData defaultData = new DrawData.RotationalData(null, 0);
        public DrawData.RotationalData dataNorth = new DrawData.RotationalData(new Rot4?(Rot4.North), 0);
        public DrawData.RotationalData dataEast = new DrawData.RotationalData(new Rot4?(Rot4.East), 0);
        public DrawData.RotationalData dataSouth = new DrawData.RotationalData(new Rot4?(Rot4.South), 0);
        public DrawData.RotationalData dataWest = new DrawData.RotationalData(new Rot4?(Rot4.West), 0);

        public DrawData GetDrawData()
        {
            return DrawData.NewWithData(new DrawData.RotationalData[]
                {
                    defaultData,
                    dataSouth,
                    dataEast,
                    dataWest,
                    dataNorth
                });
        }
    }
}
