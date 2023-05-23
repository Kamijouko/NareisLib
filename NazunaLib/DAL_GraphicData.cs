using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class DAL_GraphicData : GraphicData
    {
        public Graphic curGraphic;

		public List<Graphic> curGraphics;

		public Camera curCamera;

		public DAL_GraphicData(GraphicData g, string suffix = "", Gender gender = Gender.None)
		{
			CopyFrom(g);
			curGraphic = null;
			if (!suffix.NullOrEmpty())
			{
				if (gender != Gender.None)
				{
					texPath += ("_" + gender.ToString() + suffix);
				}
				else
				{
					texPath += suffix;
				}
			}
			
		}

		public Graphic Graph
		{
			get
			{
				if (curGraphic == null || curGraphic.path != texPath)
				{
					DALInit(curCamera);
				}
				return curGraphic;
			}
		}

		public void DALInit(Camera camera)
		{
			if (this.graphicClass == null)
			{
				curGraphic = null;
				return;
			}
			ShaderTypeDef cutout = this.shaderType;
			if (cutout == null)
			{
				cutout = ShaderTypeDefOf.Cutout;
			}
			Shader shader = cutout.Shader;
			if (this.graphicClass == typeof(DAL_Graphic_Dynamic))
			{
				GraphicRequest graphicRequest = new GraphicRequest(graphicClass, "", shader, drawSize, color, colorTwo, this, 0, shaderParameters, null);
				curGraphic = GetInner(graphicRequest, camera);
			}
			else
			{
				curGraphic = GraphicDatabase.Get(this.graphicClass, this.texPath, shader, this.drawSize, this.color, this.colorTwo, this, this.shaderParameters, null);
			}
			if (this.onGroundRandomRotateAngle > 0.01f)
			{
				this.curGraphic = new Graphic_RandomRotated(curGraphic, this.onGroundRandomRotateAngle);
			}
			if (this.Linked)
			{
				this.curGraphic = GraphicUtility.WrapLinked(curGraphic, this.linkType);
			}
		}

		private static DAL_Graphic_Dynamic GetInner(GraphicRequest req, Camera camera)
		{
			req.renderQueue = ((req.renderQueue == 0 && req.graphicData != null) ? req.graphicData.renderQueue : req.renderQueue);
			DAL_Graphic_Dynamic graphic;
			graphic = Activator.CreateInstance<DAL_Graphic_Dynamic>();
			graphic.camera = camera;
			graphic.Init(req);
			return graphic;
		}
	}
}
