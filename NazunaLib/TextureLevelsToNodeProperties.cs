using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class TextureLevelsToNodeProperties : PawnRenderNodeProperties
    {
        public TextureLevelsToNodeProperties(TextureLevels level, MultiTexBatch batch) 
        {
            textureLevels = level;
            multiTexBatch = batch;
            debugLabel = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
            workerClass = GetWorkerClass(multiTexBatch.originalDefName);

        }

        public Type GetWorkerClass(string defName)
        {
            Def def = DefDatabase<Def>.GetNamed(defName);
            if (def.GetType() == typeof(ThingDef))
            {
                ThingDef thingDef = (ThingDef)def;
                if (thingDef.thingClass == typeof(Apparel))
                {
                    if (thingDef.apparel.parentTagDef == PawnRenderNodeTagDefOf.ApparelBody)
                        return typeof(PawnRenderNodeWorker_Apparel_Body);
                    if (thingDef.apparel.parentTagDef == PawnRenderNodeTagDefOf.ApparelHead)
                        return typeof(PawnRenderNodeWorker_Apparel_Head);
                }
                
            }
            if (def.GetType() == typeof(HeadTypeDef))
                return typeof(PawnRenderNodeWorker_Head);
            if (def.GetType() == typeof(BodyTypeDef))
                return typeof(PawnRenderNodeWorker_Body);
            else
                return typeof(PawnRenderNodeWorker);

        }
        public TextureLevels textureLevels;
        public MultiTexBatch multiTexBatch;
    }
}
