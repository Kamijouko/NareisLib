using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Verse;

namespace NareisLib
{
    public class TextureLevelsToNode : PawnRenderNode
    {
        public TextureLevelsToNode(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public TextureLevelsToNodeProperties CurProps
        {
            get
            {
                return (TextureLevelsToNodeProperties)Props;
            }
        }

        public TextureLevels textureLevels;

        public MultiTexBatch multiTexBatch;

        public MultiRenderComp comp;

        /*public override Mesh GetMesh(PawnDrawParms parms)
        {
            Mesh bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (parms.pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(parms.pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(parms.pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(parms.pawn);
            }
            else
                bodyMesh = base.GetMesh(parms);

            Mesh mesh = null;
            if (textureLevels.meshSize != Vector2.zero)
            {
                mesh = NareisLib_MeshPool.GetMeshSetForWidth(textureLevels.meshSize.x, textureLevels.meshSize.y).MeshAt(parms.facing, textureLevels.flipped);
            }
            else
            {
                if (parms.pawn.RaceProps.Humanlike)
                {
                    switch (textureLevels.meshType)
                    {
                        case "Body":
                            mesh = bodyMeshSet.MeshAt(parms.facing, textureLevels.flipped);
                            break;
                        case "Head":
                            mesh = headMeshSet.MeshAt(parms.facing, textureLevels.flipped);
                            break;
                        case "Hair":
                            mesh = hairMeshSet.MeshAt(parms.facing, textureLevels.flipped);
                            break;
                    }
                }
                else
                    mesh = bodyMesh;
            }
            return mesh;
        }*/


        public override Graphic GraphicFor(Pawn pawn)
        {
            AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            Color colorOne;
            Color colorTwo;
            switch (textureLevels.renderLayer)
            {
                case (TextureRenderLayer.BottomOverlay):
                    colorOne = NareisLib_ColorResolve.ResolveColorFirst(alienComp, textureLevels, pawn);
                    colorTwo = alienComp != null ? alienComp.GetChannel("hair").second : Color.white;
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecond(colorTwo, alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Body):
                    colorOne = alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
                    colorTwo = alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstOne(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondOne(colorTwo, alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Hand):
                    colorOne = alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
                    colorTwo = alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstOne(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondOne(colorTwo, alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.BottomShell):
                    colorOne = apparel.DrawColor;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstTwo(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Apparel):
                    colorOne = apparel.DrawColor;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstTwo(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.FrontShell):
                    colorOne = apparel == null ? NareisLib_ColorResolve.ResolveColorFirstOne(Color.white, alienComp, textureLevels, pawn) : apparel.DrawColor;
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Head):
                    colorOne = alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
                    colorTwo = alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
                    break;
                case (TextureRenderLayer.FaceMask):
                    colorOne = apparel.DrawColor;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstTwo(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.BottomHair):
                    colorOne = alienComp != null ? alienComp.GetChannel("hair").first : pawn.story.HairColor;
                    colorTwo = alienComp != null ? alienComp.GetChannel("hair").second : Color.white;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirst(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecond(colorTwo, alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Hair):
                    colorOne = alienComp != null ? alienComp.GetChannel("hair").first : pawn.story.HairColor;
                    colorTwo = alienComp != null ? alienComp.GetChannel("hair").second : Color.white;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirst(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecond(colorTwo, alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.HeadMask):
                    colorOne = apparel.DrawColor;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstTwo(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Hat):
                    colorOne = apparel.DrawColor;
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstTwo(colorOne, alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                case (TextureRenderLayer.Overlay):
                    colorOne = NareisLib_ColorResolve.ResolveColorFirstTwo(alienComp, textureLevels, pawn);
                    colorTwo = NareisLib_ColorResolve.ResolveColorSecondTwo(alienComp, textureLevels, pawn);
                    break;
                default:
                    colorOne = ColorFor(pawn);
                    colorTwo = ColorFor(pawn);
                    break;
            }
            string condition = "";
            RotDrawMode bodyDrawType = pawn.Drawer.renderer.CurRotDrawMode;
            if (textureLevels.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                condition = "Rotting";
            if (textureLevels.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                condition = "Dessicated";
            string bodyType = "";
            string headType = "";
            if (textureLevels.useBodyType)
                bodyType = pawn.story.bodyType.defName;
            else if (textureLevels.useHeadType)
                headType = pawn.story.headType.defName;

            return textureLevels.GetGraphic(multiTexBatch.keyName, colorOne, colorTwo, condition, bodyType, headType);
            //return base.GraphicFor(pawn);
        }


    }
}
