using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class TextureLevelsToNodeProperties : PawnRenderNodeProperties
    {
        public TextureLevelsToNodeProperties() 
        {
            
        }
        
        public void InitProperties(TextureLevels level, MultiTexBatch batch)
        {
            textureLevels = level;
            multiTexBatch = batch;
            debugLabel = $"{level.textureLevelsName}_{level.keyName}";
            workerClass = level.renderWorker ?? GetWorkerClass(multiTexBatch.originalDefName, multiTexBatch.originalDefClass);
            parentTagDef = level.renderParentNodeTagDef ?? GetRootNodeTagDef(textureLevels, level.renderLayer);
            if (level.hasRotting)
                rotDrawMode |= RotDrawMode.Fresh | RotDrawMode.Rotting;
            if (level.hasDessicated)
                rotDrawMode |= RotDrawMode.Dessicated;
            if (level.useStaticColor)
                color = level.color;
            if (level.useHairColor)
                colorType = AttachmentColorType.Hair;
            if (level.useBodyColor)
                colorType = AttachmentColorType.Skin;
            baseLayer = level.baseLayer;
            drawSize = level.drawSize;
            flipGraphic = level.flipped;
            oppositeFacingLayerWhenFlipped = level.oppositeFacingLayerWhenFlipped;
            nodeClass = typeof(TextureLevelsToNode);
            overlayLayer = level.overlayLayer;
            overlayOverApparel = level.overlayOverApparel;
            if (level.meshSize != Vector2.zero)
                overrideMeshSize = level.meshSize;
            pawnType = level.pawnType;
            rotateIndependently = level.rotateIndependently;
            shaderTypeDef = level.shaderType;
            side = level.side;
            skipFlag = level.skipFlag;
            subworkerClasses = level.subworkerClasses;
            tagDef = level.tagDef;

            if (level.drawData != null)
            {
                drawData = DrawData.NewWithData(new DrawData.RotationalData[]
                {
                    new DrawData.RotationalData(new Rot4?(Rot4.South), level.drawData.southLayer),
                    new DrawData.RotationalData(new Rot4?(Rot4.East), level.drawData.eastLayer),
                    new DrawData.RotationalData(new Rot4?(Rot4.West), level.drawData.westLayer),
                    new DrawData.RotationalData(new Rot4?(Rot4.North), level.drawData.northLayer)
                });
            }

            if (workerClass != typeof(PawnRenderNodeWorker_TextureLevels) && !subworkerClasses.Contains(typeof(TextureLevelsToNodeSubWorker)))
                subworkerClasses.Add(typeof(TextureLevelsToNodeSubWorker));
        }

        public PawnRenderNodeTagDef GetRootNodeTagDef(TextureLevels level, TextureRenderLayer layer)
        {
            /*if (level.usePublicYOffset || level.useStaticYOffset)
                return DefDatabase<PawnRenderNodeTagDef>.GetNamed("Root");*/
            switch (layer)
            {
                case TextureRenderLayer.BottomOverlay:
                    return DefDatabase<PawnRenderNodeTagDef>.GetNamed("Root");
                case TextureRenderLayer.BottomHair:
                    return PawnRenderNodeTagDefOf.Head;
                case TextureRenderLayer.BottomShell:
                    return PawnRenderNodeTagDefOf.ApparelBody;
                case TextureRenderLayer.Body:
                    return PawnRenderNodeTagDefOf.Body;
                case TextureRenderLayer.Apparel:
                    return PawnRenderNodeTagDefOf.ApparelBody;
                case TextureRenderLayer.Hand:
                    return PawnRenderNodeTagDefOf.Body;
                case TextureRenderLayer.Head:
                    return PawnRenderNodeTagDefOf.Head;
                case TextureRenderLayer.FaceMask:
                    return PawnRenderNodeTagDefOf.ApparelHead;
                case TextureRenderLayer.Hair:
                    return PawnRenderNodeTagDefOf.Head;
                case TextureRenderLayer.HeadMask:
                    return PawnRenderNodeTagDefOf.ApparelHead;
                case TextureRenderLayer.FrontShell:
                    return PawnRenderNodeTagDefOf.ApparelBody;
                case TextureRenderLayer.Hat:
                    return PawnRenderNodeTagDefOf.ApparelHead;
                case TextureRenderLayer.Overlay:
                    return DefDatabase<PawnRenderNodeTagDef>.GetNamed("Root");


                default: return DefDatabase<PawnRenderNodeTagDef>.GetNamed("Root");
            }
        }

        public Type GetWorkerClass(string defName, Type type)
        {
            Def def = null;
            if (type != typeof(HandTypeDef))
                def = GenDefDatabase.GetDef(type, defName);
            if (type == typeof(ThingDef) && def != null)
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
            if (type == typeof(HeadTypeDef))
                return typeof(PawnRenderNodeWorker_Head);
            if (type == typeof(BodyTypeDef))
                return typeof(PawnRenderNodeWorker_Body);
            if (type == typeof(HandTypeDef))
                return typeof(PawnRenderNodeWorker_Hand);
            if (type == typeof(HairDef))
                return typeof(PawnRenderNodeWorker_TextureLevels);
            else
                return typeof(PawnRenderNodeWorker_TextureLevels);
        }
        public TextureLevels textureLevels;
        public MultiTexBatch multiTexBatch;
    }
}
