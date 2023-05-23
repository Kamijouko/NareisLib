using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class DAL_PawnExpressionData
    {
        public string partName;

        public bool hasMovingExpression = false;
        public bool hasTransFrame = false;
        public bool isMoving = false;
        public bool isBlink = false;
        public bool isDamage = false;
        

        //public bool separateEyeMouse = false;
        //public bool hasRightEye = false;

        public Gender gender = Gender.None;
        public string corwnType = "";
        public DAL_PawnExpressionPartFlags partFlag = DAL_PawnExpressionPartFlags.Facial;
        public Dictionary<Rot4, int> frameCount = new Dictionary<Rot4, int>() { {Rot4.North, 1}, { Rot4.East, 1 }, { Rot4.South, 1 }, { Rot4.West, 1 } };
        public Dictionary<Rot4, int> transFrameCount = new Dictionary<Rot4, int>() { { Rot4.North, 1 }, { Rot4.East, 1 }, { Rot4.South, 1 }, { Rot4.West, 1 } };
        public GraphicData frame;
        public GraphicData transFrame;
    }
}
