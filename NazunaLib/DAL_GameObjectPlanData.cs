using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace NazunaLib
{
    public class DAL_GameObjectPlanData
    {
        public string name = "";

        public DAL_TypeOfObjectGraph type = DAL_TypeOfObjectGraph.Thing;

        public bool canHolding = false;

        public bool canAiming = false;

        public float yOffset = 0f;

        public bool autoPlay = false;

        public bool canRotation = false;

        public bool isMoveable = false;

        public Type verbClass;
    }
}
