using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace NareisLib
{
    public class DAL_CompProperties_HeadTurning : CompProperties
    {
        public DAL_CompProperties_HeadTurning()
        {
            this.compClass = typeof(DAL_CompHeadTurning);
        }

        public bool enable = false;

        public float attentionDist = 10f;

        public int turningLimit = 3;

        public float coolDownSeconds = 7f;

        public float maxTurningSeconds = 5f;

        public float minTurningSeconds = 3f;
    }
}
