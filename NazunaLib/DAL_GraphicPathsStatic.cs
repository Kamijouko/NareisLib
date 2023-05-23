using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using AlienRace;
using UnityEngine;

namespace NareisLib
{
	public static class DAL_GraphicPathsStatic
	{
		public static GraphicPaths GetDALCurrentGraphicPath(this List<DAL_GraphicPaths> list, string breastSize, LifeStageDef lifeStageDef)
		{
			int count = list.Count();

			if (count < 1)
			{
				return null;
			}
			else
			{
				if (count == 1 || breastSize == "None")
				{
					return list[0].graphicPaths.FirstOrDefault(delegate (GraphicPaths gp)
					{
						List<LifeStageDef> lifeStageDefs = null;
						return lifeStageDefs != null && lifeStageDefs.Contains(lifeStageDef);
					}) ?? list[0].graphicPaths.First<GraphicPaths>();
				}
				else
				{
					List<GraphicPaths> lgp = new List<GraphicPaths>();
					for (int i = 0; i < count; i++)
					{
						if (list[i].breastSize == breastSize)
						{
							lgp = list[i].graphicPaths;
							break;
						}
					}
					return lgp.FirstOrDefault(delegate (GraphicPaths gp)
					{
						List<LifeStageDef> lifeStageDefs = null;
						return lifeStageDefs != null && lifeStageDefs.Contains(lifeStageDef);
					}) ?? lgp.First<GraphicPaths>();
				}
			}

			/*return list.FirstOrDefault(delegate (DAL_GraphicPaths dgp)
			{
				string bs = dgp.breastSize;
				return bs != null && bs == breastSize;
			}).graphicPaths.FirstOrDefault(delegate (GraphicPaths gp)
			{
				List<LifeStageDef> lsds = gp.lifeStageDefs;
				return lsds != null && lsds.Contains(lifeStageDef);
			}) ?? list.First<DAL_GraphicPaths>().graphicPaths.First<GraphicPaths>();*/
		}

		public static GraphicPaths GetDALCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef)
		{
			return list.FirstOrDefault(delegate (GraphicPaths gp)
			{
				List<LifeStageDef> lifeStageDefs = null;
				return lifeStageDefs != null && lifeStageDefs.Contains(lifeStageDef);
			}) ?? list.First<GraphicPaths>();
		}

		public static bool TryGetDALGraphicApparel(this Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec, string gender = null, string breastSize = null, Shader shader = null, DAL_ApparelForDifferentRace apparelForDifferentRace = null, DAL_ReplaceApparelProperties replaceApparelProperties = null)
		{
			if (bodyType == null)
			{
				Log.Error("Getting apparel graphic with undefined body type.");
				bodyType = BodyTypeDefOf.Thin;
			}
			if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
			{
				rec = new ApparelGraphicRecord(null, null);
				return false;
			}

			DAL_ApparelThingDef apparelDef = apparel.def as DAL_ApparelThingDef;
			if (apparelDef != null)
			{
				if (replaceApparelProperties == null)
				{
					if (apparelForDifferentRace == null)
					{
						string path01;
						if (apparelDef.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelDef.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)
						{
							if (gender != null)
							{
								DAL_GenderReplacePaths genderReplacePaths = apparelDef.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
								{
									return grp.gender == gender;
								});
								if (genderReplacePaths != null)
								{
									path01 = genderReplacePaths.replaceWornGraphicPath;
								}
								else
								{
									path01 = apparelDef.apparel.wornGraphicPath;
								}
							}
							else
							{
								path01 = apparelDef.apparel.wornGraphicPath;
							}
							if (apparelDef.defaultWornApparelShader != null)
							{
								shader = apparelDef.defaultWornApparelShader.Shader;
							}
							else
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path01, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}
						else
						{
							if (breastSize != null && gender == null)
							{
								DAL_BreastSizeReplacePaths breastSizeReplacePaths = apparelDef.breastSizeReplacePaths.FirstOrDefault(delegate (DAL_BreastSizeReplacePaths afdr)
								{
									return afdr.breastSize == breastSize;
								});
								if (breastSizeReplacePaths != null)
								{
									path01 = breastSizeReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
									shader = breastSizeReplacePaths.shader.Shader;
								}
								else
								{
									path01 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
								if (shader == null)
								{
									shader = ShaderDatabase.Cutout;
									if (apparel.def.apparel.useWornGraphicMask)
									{
										shader = ShaderDatabase.CutoutComplex;
									}
								}
								Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path01, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
								rec = new ApparelGraphicRecord(graphic, apparel);
								return true;

							}

							if (gender != null && breastSize == null)
							{
								DAL_GenderReplacePaths genderReplacePaths = apparelDef.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
								{
									return grp.gender == gender;
								});
								if (genderReplacePaths != null)
								{
									path01 = genderReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
									shader = genderReplacePaths.shader.Shader;
								}
								else
								{
									path01 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
								if (shader == null)
								{
									shader = ShaderDatabase.Cutout;
									if (apparel.def.apparel.useWornGraphicMask)
									{
										shader = ShaderDatabase.CutoutComplex;
									}
								}
								Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path01, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
								rec = new ApparelGraphicRecord(graphic, apparel);
								return true;
							}

							if (gender == null && breastSize == null)
							{
								path01 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
								if (shader == null)
								{
									shader = ShaderDatabase.Cutout;
									if (apparel.def.apparel.useWornGraphicMask)
									{
										shader = ShaderDatabase.CutoutComplex;
									}
								}
								Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path01, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
								rec = new ApparelGraphicRecord(graphic, apparel);
								return true;
							}
						}

						rec = new ApparelGraphicRecord(null, null);
						return false;
					}
					else
					{
						string path02;
						if (apparelDef.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelDef.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)
						{
							if (gender != null)
							{
								DAL_GenderReplacePaths genderReplacePaths = apparelForDifferentRace.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
								{
									return grp.gender == gender;
								});
								if (genderReplacePaths != null)
								{
									path02 = genderReplacePaths.replaceWornGraphicPath;
									shader = genderReplacePaths.shader.Shader;
								}
								else
								{
									path02 = apparelForDifferentRace.defaultReplaceWornGraphicPath;
									if (apparelForDifferentRace.defaultReplaceWornGraphicPath == null)
									{
										path02 = apparelDef.apparel.wornGraphicPath;
									}
								}
							}
							else
							{
								path02 = apparelForDifferentRace.defaultReplaceWornGraphicPath;
								if (apparelForDifferentRace.defaultReplaceWornGraphicPath == null)
								{
									path02 = apparelDef.apparel.wornGraphicPath;
								}
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path02, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}
						else
						{
							if (breastSize != null && gender == null)
							{
								DAL_BreastSizeReplacePaths breastSizeReplacePaths = apparelForDifferentRace.breastSizeReplacePaths.FirstOrDefault(delegate (DAL_BreastSizeReplacePaths bsrp)
								{
									return bsrp.breastSize == breastSize;
								});
								if (breastSizeReplacePaths != null)
								{
									path02 = breastSizeReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
									shader = breastSizeReplacePaths.shader.Shader;
								}
								else
								{
									path02 = apparelForDifferentRace.defaultReplaceWornGraphicPath + "_" + bodyType.defName;
									if (apparelForDifferentRace.defaultReplaceWornGraphicPath == null)
									{
										path02 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
									}
								}
								if (shader == null)
								{
									shader = ShaderDatabase.Cutout;
									if (apparel.def.apparel.useWornGraphicMask)
									{
										shader = ShaderDatabase.CutoutComplex;
									}
								}
								Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path02, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
								rec = new ApparelGraphicRecord(graphic, apparel);
								return true;
							}

							if (breastSize == null && gender != null)
							{
								DAL_GenderReplacePaths genderReplacePaths = apparelForDifferentRace.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
								{
									return grp.gender == gender;
								});
								if (genderReplacePaths != null)
								{
									path02 = genderReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
									shader = genderReplacePaths.shader.Shader;
								}
								else
								{
									path02 = apparelForDifferentRace.defaultReplaceWornGraphicPath + "_" + bodyType.defName;
									if (apparelForDifferentRace.defaultReplaceWornGraphicPath == null)
									{
										path02 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
									}
								}
								if (shader == null)
								{
									shader = ShaderDatabase.Cutout;
									if (apparel.def.apparel.useWornGraphicMask)
									{
										shader = ShaderDatabase.CutoutComplex;
									}
								}
								Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path02, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
								rec = new ApparelGraphicRecord(graphic, apparel);
								return true;
							}

							if (breastSize == null && gender == null)
							{
								path02 = apparelForDifferentRace.defaultReplaceWornGraphicPath + "_" + bodyType.defName;
								if (apparelForDifferentRace.defaultReplaceWornGraphicPath == null)
								{
									path02 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
								if (shader == null)
								{
									shader = ShaderDatabase.Cutout;
									if (apparel.def.apparel.useWornGraphicMask)
									{
										shader = ShaderDatabase.CutoutComplex;
									}
								}
								Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path02, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
								rec = new ApparelGraphicRecord(graphic, apparel);
								return true;
							}

							rec = new ApparelGraphicRecord(null, null);
							return false;
						}
					}
				}
				else
				{
					string path03;
					if (apparelDef.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelDef.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)
					{
						if (gender != null)
						{
							DAL_GenderReplacePaths genderReplacePaths = replaceApparelProperties.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
							{
								return grp.gender == gender;
							});
							if (genderReplacePaths != null)
							{
								path03 = genderReplacePaths.replaceWornGraphicPath;
								shader = genderReplacePaths.shader.Shader;
							}
							else
							{
								path03 = replaceApparelProperties.defaultWornGraphicPath;
								if (replaceApparelProperties.defaultWornGraphicPath == null)
								{
									path03 = apparelDef.apparel.wornGraphicPath;
								}
							}
						}
						else
						{
							path03 = replaceApparelProperties.defaultWornGraphicPath;
							if (replaceApparelProperties.defaultWornGraphicPath == null)
							{
								path03 = apparelDef.apparel.wornGraphicPath;
							}
						}
						if (shader == null)
						{
							shader = ShaderDatabase.Cutout;
							if (apparel.def.apparel.useWornGraphicMask)
							{
								shader = ShaderDatabase.CutoutComplex;
							}
						}
						Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path03, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
						rec = new ApparelGraphicRecord(graphic, apparel);
						return true;
					}
					else
					{
						if (breastSize != null && gender == null)
						{
							DAL_BreastSizeReplacePaths breastSizeReplacePaths = replaceApparelProperties.breastSizeReplacePaths.FirstOrDefault(delegate (DAL_BreastSizeReplacePaths bsrp)
							{
								return bsrp.breastSize == breastSize;
							});
							if (breastSizeReplacePaths != null)
							{
								path03 = breastSizeReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
								shader = breastSizeReplacePaths.shader.Shader;
							}
							else
							{
								path03 = replaceApparelProperties.defaultWornGraphicPath + "_" + bodyType.defName;
								if (replaceApparelProperties.defaultWornGraphicPath == null)
								{
									path03 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path03, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}

						if (breastSize == null && gender != null)
						{
							DAL_GenderReplacePaths genderReplacePaths = replaceApparelProperties.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
							{
								return grp.gender == gender;
							});
							if (genderReplacePaths != null)
							{
								path03 = genderReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
								shader = genderReplacePaths.shader.Shader;
							}
							else
							{
								path03 = replaceApparelProperties.defaultWornGraphicPath + "_" + bodyType.defName;
								if (replaceApparelProperties.defaultWornGraphicPath == null)
								{
									path03 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path03, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}

						if (breastSize == null && gender == null)
						{
							path03 = replaceApparelProperties.defaultWornGraphicPath + "_" + bodyType.defName;
							if (replaceApparelProperties.defaultWornGraphicPath == null)
							{
								path03 = apparelDef.apparel.wornGraphicPath + "_" + bodyType.defName;
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path03, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}

						rec = new ApparelGraphicRecord(null, null);
						return false;
					}
				}
			}
			else
			{
				if (replaceApparelProperties == null)
				{
					string path04;
					if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.def.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)
					{
						path04 = apparel.def.apparel.wornGraphicPath;
					}
					else
					{
						path04 = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
					}
					shader = ShaderDatabase.Cutout;
					if (apparel.def.apparel.useWornGraphicMask)
					{
						shader = ShaderDatabase.CutoutComplex;
					}
					Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path04, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
					rec = new ApparelGraphicRecord(graphic, apparel);
					return true;
				}
				else
				{
					string path05;
					if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.def.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)
					{
						if (gender != null)
						{
							DAL_GenderReplacePaths genderReplacePaths = replaceApparelProperties.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
							{
								return grp.gender == gender;
							});
							if (genderReplacePaths != null)
							{
								path05 = genderReplacePaths.replaceWornGraphicPath;
								shader = genderReplacePaths.shader.Shader;
							}
							else
							{
								path05 = replaceApparelProperties.defaultWornGraphicPath;
								if (replaceApparelProperties.defaultWornGraphicPath == null)
								{
									path05 = apparel.def.apparel.wornGraphicPath;
								}
							}
						}
						else
						{
							path05 = replaceApparelProperties.defaultWornGraphicPath;
							if (replaceApparelProperties.defaultWornGraphicPath == null)
							{
								path05 = apparel.def.apparel.wornGraphicPath;
							}
						}
						if (shader == null)
						{
							shader = ShaderDatabase.Cutout;
							if (apparel.def.apparel.useWornGraphicMask)
							{
								shader = ShaderDatabase.CutoutComplex;
							}
						}
						Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path05, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
						rec = new ApparelGraphicRecord(graphic, apparel);
						return true;
					}
					else
					{
						if (breastSize != null && gender == null)
						{
							DAL_BreastSizeReplacePaths breastSizeReplacePaths = replaceApparelProperties.breastSizeReplacePaths.FirstOrDefault(delegate (DAL_BreastSizeReplacePaths bsrp)
							{
								return bsrp.breastSize == breastSize;
							});
							if (breastSizeReplacePaths != null)
							{
								path05 = breastSizeReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
								shader = breastSizeReplacePaths.shader.Shader;
							}
							else
							{
								path05 = replaceApparelProperties.defaultWornGraphicPath + "_" + bodyType.defName;
								if (replaceApparelProperties.defaultWornGraphicPath == null)
								{
									path05 = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path05, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}

						if (breastSize == null && gender != null)
						{
							DAL_GenderReplacePaths genderReplacePaths = replaceApparelProperties.genderReplacePaths.FirstOrDefault(delegate (DAL_GenderReplacePaths grp)
							{
								return grp.gender == gender;
							});
							if (genderReplacePaths != null)
							{
								path05 = genderReplacePaths.replaceWornGraphicPath + "_" + bodyType.defName;
								shader = genderReplacePaths.shader.Shader;
							}
							else
							{
								path05 = replaceApparelProperties.defaultWornGraphicPath + "_" + bodyType.defName;
								if (replaceApparelProperties.defaultWornGraphicPath == null)
								{
									path05 = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
								}
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path05, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}

						if (breastSize == null && gender == null)
						{
							path05 = replaceApparelProperties.defaultWornGraphicPath + "_" + bodyType.defName;
							if (replaceApparelProperties.defaultWornGraphicPath == null)
							{
								path05 = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
							}
							if (shader == null)
							{
								shader = ShaderDatabase.Cutout;
								if (apparel.def.apparel.useWornGraphicMask)
								{
									shader = ShaderDatabase.CutoutComplex;
								}
							}
							Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path05, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
							rec = new ApparelGraphicRecord(graphic, apparel);
							return true;
						}

						rec = new ApparelGraphicRecord(null, null);
						return false;
					}
				}

			}
		}

		public static HairDef GetRandomHairDefFromHairTag(this string hairTag, string hairGender = null)
		{
			List<HairDef> list = new List<HairDef>();
			foreach (HairDef hd in DefDatabase<HairDef>.AllDefs)
			{
				if (hd.styleTags.Contains(hairTag))
				{
					if (hairGender == null)
					{
						list.Add(hd);
					}
					else
					{
						if (hd.styleGender.ToString() == hairGender)
						{
							list.Add(hd);
						}
					}
				}
			}
			return !list.NullOrEmpty() ? list.RandomElement() : null;
		}

	}
}
