using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class DAL_Graphic_Dynamic : Graphic_Single
    {
		public Camera camera;
        public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			this.path = req.path;
			this.maskPath = req.maskPath;
			this.color = req.color;
			this.colorTwo = req.colorTwo;
			this.drawSize = req.drawSize;
			this.mat = new Material(req.shader);
			this.mat.name = req.shader.name;
			this.mat.mainTexture = camera.targetTexture;
			this.mat.color = color;
			if (ContentFinder<Texture2D>.Get(this.maskPath.NullOrEmpty() ? (this.path + Graphic_Single.MaskSuffix) : this.maskPath, false) != null)
			{
				this.mat.SetTexture(ShaderPropertyIDs.MaskTex, ContentFinder<Texture2D>.Get(this.maskPath.NullOrEmpty() ? (this.path + Graphic_Single.MaskSuffix) : this.maskPath, false));
				this.mat.SetColor(ShaderPropertyIDs.ColorTwo, colorTwo);
			}
			if (req.renderQueue != 0)
			{
				this.mat.renderQueue = req.renderQueue;
			}
			if (!req.shaderParameters.NullOrEmpty<ShaderParameter>())
			{
				for (int i = 0; i < req.shaderParameters.Count; i++)
				{
					req.shaderParameters[i].Apply(this.mat);
				}
			}
			if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
			{
				WindManager.Notify_PlantMaterialCreated(this.mat);
			}
		}

		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			return this.mat;
		}

		public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
		{
			Texture2D mask = null;
			if (this.mat.HasProperty(ShaderPropertyIDs.MaskTex))
			{
				mask = (Texture2D)this.mat.GetTexture(ShaderPropertyIDs.MaskTex);
			}
			GlobalTextureAtlasManager.TryInsertStatic(groupKey, (Texture2D)this.mat.mainTexture, mask);
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			GraphicRequest graphicRequest = new GraphicRequest(typeof(DAL_Graphic_Dynamic), "", newShader, drawSize, newColor, newColorTwo, data, 0, null, null);
			graphicRequest.renderQueue = ((graphicRequest.renderQueue == 0 && graphicRequest.graphicData != null) ? graphicRequest.graphicData.renderQueue : graphicRequest.renderQueue);
			DAL_Graphic_Dynamic graphic;
			graphic = Activator.CreateInstance<DAL_Graphic_Dynamic>();
			graphic.camera = camera;
			graphic.Init(graphicRequest);
			return graphic;
		}
	}
}
