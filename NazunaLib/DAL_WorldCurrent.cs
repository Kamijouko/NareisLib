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
    public static class DAL_WorldCurrent
    {
        public static DAL_DynamicGameObjectManager GOM
        {
            get
            {
                return DAL_WorldCurrent.gameObjectManager;
            }
            set
            {
                DAL_WorldCurrent.gameObjectManager = value;
            }
        }

        private static DAL_DynamicGameObjectManager gameObjectManager;
    }
}
