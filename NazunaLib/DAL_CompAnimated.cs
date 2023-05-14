 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.IO;
//using CompAnimated;

namespace NazunaLib
{
    public class DAL_CompAnimated : ThingComp
	{
		public int ticks = -1;
		public bool dirty;
		public bool initialization = false;

		public Pawn pawn = null;
		public bool hasBreastOption = false;

		public bool shouldUpdateHeadFrame = false;
		public bool shouldUpdateHairFrame = false;
		public bool shouldUpdateBodyFrame = false;
		public bool shouldUpdateApparelFrame = false;
		public bool shouldUpdateThingFrame = false;
		public bool shouldUpdateCustomFrame = false;
		public bool shouldUpdateGameObject = false;

		public List<DAL_CompAnimatedFramePath> headList = new List<DAL_CompAnimatedFramePath>();
		public List<DAL_CompAnimatedFramePath> bodyList = new List<DAL_CompAnimatedFramePath>();
		public List<DAL_CompAnimatedFramePath> hairList = new List<DAL_CompAnimatedFramePath>();
		public List<DAL_CompAnimatedFramePath> apparelList = new List<DAL_CompAnimatedFramePath>();
		public List<DAL_CompAnimatedFramePath> thingList = new List<DAL_CompAnimatedFramePath>();
		public List<DAL_CompAnimatedFramePath> customList = new List<DAL_CompAnimatedFramePath>();

		public List<DAL_GraphicInfo> headGraphicList = new List<DAL_GraphicInfo>();
		public List<DAL_GraphicInfo> nakedGraphicList = new List<DAL_GraphicInfo>();
		public List<DAL_GraphicInfo> hairGraphicList = new List<DAL_GraphicInfo>();
		public List<DAL_GraphicInfo> apparelGraphicList = new List<DAL_GraphicInfo>();
		public DAL_GraphicInfo curThingPrimaryGraphic;
		public List<DAL_GraphicInfo> curThingGraphics = new List<DAL_GraphicInfo>();
		public List<DAL_GraphicInfo> curCustomGraphics = new List<DAL_GraphicInfo>();

		public Dictionary<string, BodyPartRecord> curAddonBodyParts = new Dictionary<string, BodyPartRecord>();

		public Dictionary<string, int> curIndexArray = new Dictionary<string, int>();

		public GameObject curGameObject = null;
		public Dictionary<string, GameObject> subGameObjectArray = new Dictionary<string, GameObject>();
		public bool subGameObjectInitialized = false;

		public bool Fixed = false;
		public bool ApparelFixed = false;
		/*public Pawn EquipmentPawn = null;*/

		public override void PostExposeData()
		{
			base.PostExposeData();
		}

		public void CheckHediffTick()
		{
			
		}

		public override void CompTick()
        {
			base.CompTick();
			DALDefaultGraphic();
			DALResolveGameObject();
			//Messages.Message("ticked", null, MessageTypeDefOf.ThreatBig, false);

		}

		public void BodyAddonChecker()
		{

		}

		public override void PostDraw()
		{
			
			Pawn pawn = null;
			bool parentParentIsPawn = DAL_CompAnimated.ParentAsPawn(this.parent, out pawn);//是装备，返回装备的人
			if (curCustomGraphics.NullOrEmpty())
			{
				return;
			}
			base.PostDraw();
			if (parentParentIsPawn)
			{
				foreach (DAL_GraphicInfo n in curCustomGraphics)
				{
					OtherDALRender(n.graphic, n.yOffSet, pawn);
				}
				return;
			}
			foreach (DAL_GraphicInfo n in curCustomGraphics)
			{
				DALRender(n.graphic, n.yOffSet);
			}
			return;
		}

		public virtual void DALRender(Graphic graph, float y)
		{
			Vector3 drawPos = this.parent.DrawPos;
			drawPos.y = drawPos.y + y;
			graph.Draw(drawPos, Rot4.North, this.parent, 0f);
		}

		public virtual void OtherDALRender(Graphic graph, float y, ThingWithComps thing)
		{
			Vector3 drawPos = thing.DrawPos;
			drawPos.y = drawPos.y + y;
			graph.Draw(drawPos, Rot4.North, thing, 0f);
		}

		private void DALDefaultGraphic()
        {
			Dictionary<string, int> curIndex = DALResolveCurGraphic(parent, DALProps, ref curIndexArray, ref ticks, ref dirty, ref apparelGraphicList, ref curThingPrimaryGraphic, ref curThingGraphics, ref curCustomGraphics, ref headGraphicList, ref nakedGraphicList, ref hairGraphicList);
			if (curIndex != null)
			{
				curIndexArray = curIndex;
			}
			this.NotifyGraphChange();
		}

		private static bool AsPawn(ThingWithComps pAnimatee, out Pawn pawn)
		{
			Pawn pawn0 = pAnimatee as Pawn;
			bool flag;
			if (pawn0 != null)
			{
				flag = true;
			}
			else
			{
				flag = false;
			}
			pawn = pawn0;
			return flag;
		}

		private static bool ParentAsPawn(ThingWithComps pAnimatee, out Pawn pawn)
		{
			Pawn pawn0;
			bool flag;
			if ((pAnimatee as Apparel) != null)
			{
				
				
				if ((pAnimatee as Apparel).Wearer != null)
				{
					pawn0 = (pAnimatee as Apparel).Wearer;
					flag = true;
				}
				else
				{
					pawn0 = null;
					flag = false;
				}
			}
			else
			{
				if (pAnimatee.ParentHolder == null)
				{
					pawn = null;
					return false;
				}
				pawn0 = pAnimatee.ParentHolder as Pawn;
				if (pawn0 != null)
				{
					flag = true;
				}
				else
				{
					pawn0 = null;
					flag = false;
				}
			}

			pawn = pawn0;
			return flag;
		}


		private static bool NowAction(List<DAL_AnimateRequireVerb> list, Verb nowVerb, Rot4 rot, out List<GraphicData> frames)
		{
			int count = list.Count();
			for (int i = 0; i < count; i++)
			{
				if (nowVerb.verbProps.verbClass == list[i].requireVerb)
				{
					if (!list[i].isRot4Frames)
					{
						frames = list[i].verbWarmUpFrames;
						return true;
					}
					else
					{
						if (rot != null)
						{
							if (rot == Rot4.East)
							{
								frames = list[i].verbWarmUpFramesEast;
							}
							else if (rot == Rot4.West)
							{
								frames = list[i].verbWarmUpFramesWest;
							}
							else if (rot == Rot4.North)
							{
								frames = list[i].verbWarmUpFramesNorth;
							}
							else
							{
								frames = list[i].verbWarmUpFramesSouth;
							}
							return true;
						}
					}
				}
			}
			frames = null;
			return false;
		}

		private bool ApparelCoveredBodyAddon(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.apparel.WornApparel?.Any(x => x.def.apparel.CoversBodyPart(part)/* && x.def.apparel.bodyPartGroups.Contains(DAL_BodyPartGroupDefOf.FullCoverage)*/) == true)
			{
				return true;
			}
			return false;
		}

		private bool ApparelCoverdGraphic(Pawn pawn, DAL_CompAnimatedFramePath path)
		{
			if (pawn.apparel.WornApparel?.Any(x => x.def.apparel.bodyPartGroups.Any(y => path.bodyPartGroups.Contains(y))) == true)
			{
				return true;
			}
			return false;
		}


		public Graphic InitGraphicData(GraphicData gd)
		{
			Graphic graphic = null;
			if (gd.graphicClass != null)
			{
				ShaderTypeDef cutout = gd.shaderType;
				if (cutout == null)
				{
					cutout = ShaderTypeDefOf.Cutout;
				}
				Shader shader = cutout.Shader;
				graphic = GraphicDatabase.Get(gd.graphicClass, gd.texPath, shader, gd.drawSize, gd.color, gd.colorTwo, gd, gd.shaderParameters, null);
				if (gd.onGroundRandomRotateAngle > 0.01f)
				{
					graphic = new Graphic_RandomRotated(graphic, gd.onGroundRandomRotateAngle);
				}
				if (gd.Linked)
				{
					graphic = GraphicUtility.WrapLinked(graphic, gd.linkType);
				}
			}
			return graphic;
		}


		public void UpdateHeadFrameList()
		{
			headList = loadPawnAnimeFrameList("head", DALProps);
			shouldUpdateHeadFrame = false;
		}

		public void UpdateHairFrameList()
		{
			hairList = loadPawnAnimeFrameList("hair", DALProps);
			shouldUpdateHairFrame = false;
		}

		public void UpdateBodyFrameList()
		{
			bodyList = loadPawnAnimeFrameList("body", DALProps);
			shouldUpdateBodyFrame = false;
		}

		public void UpdateApparelFrameList()
		{
			apparelList = loadApparelAnimeFrameList(DALProps);
			shouldUpdateApparelFrame = false;
		}

		public void UpdateThingFrameList()
		{
			thingList = loadThingAnimeFrameList(DALProps);
			shouldUpdateThingFrame = false;
		}

		public void UpdateCustomFrameList()
		{
			customList = loadCustomAnimeFrameList(DALProps);
			shouldUpdateCustomFrame = false;
		}

		public int RandomByWeight(List<DAL_BodyPartSet> list)
		{
			int totalWeight = list.Sum<DAL_BodyPartSet>(i => i.weight);
			int value = UnityEngine.Random.Range(0, totalWeight);
			int x = 0;
			foreach (DAL_BodyPartSet item in list)
			{
				x += item.weight;
				if (value >= x)
				{
					return list.IndexOf(item);
				}
			}
			return 0;
		}

		//初始化
		public List<DAL_CompAnimatedFramePath> loadPawnAnimeFrameList(string str, DAL_CompProperties_Animated prop)
		{
			List<DAL_CompAnimatedFramePath> list = new List<DAL_CompAnimatedFramePath>();

			if (prop.hairPlan != null)
			{
				if (!prop.hairPlan.hairGraphicPlans.NullOrEmpty())
				{
					if (str == "hair")
					{
						foreach (DAL_CompAnimatedFrameList i in prop.hairPlan.hairGraphicPlans)
						{
							if (pawn.story.hairDef.defName == i.framePaths.hairDefName)
							{
								list.Add(i.framePaths);
							}
						}
					}
				}
			}

			if (prop.addonPlan != null)
			{
				if (!prop.addonPlan.bodyAddonPlans.NullOrEmpty())
				{
					int index = RandomByWeight(prop.addonPlan.bodyAddonPlans);
					foreach (DAL_CompAnimatedFrameList i in prop.addonPlan.bodyAddonPlans[index].bodyAddonList)
					{
						/*if (i.hasGender && i.gender != pawn.gender)
						{
							continue;
						}*/
						if ((i.onlyMale && pawn.gender != Gender.Male) || (i.onlyFemale && pawn.gender != Gender.Female))
                        {
							continue;
						}
						BodyPartRecord part = pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.customLabel == i.framePaths.bodyPartCustomName);
						if (part == null)
						{
							continue;
						}
						else if (!curAddonBodyParts.ContainsKey(i.framePaths.bodyPartCustomName))
						{
							curAddonBodyParts.Add(i.framePaths.bodyPartCustomName, part);
						}
						if (str == "hair" && i.isHair)
						{
							list.Add(i.framePaths);
							continue;
						}
						if (str == "head" && i.isHead)
						{
							list.Add(i.framePaths);
							continue;
						}
						if (str == "body" && i.isBody)
						{
							list.Add(i.framePaths);
							continue;
						}
						if (str == "headAddon" && i.isHeadAddon)
						{
							list.Add(i.framePaths);
							continue;
						}
						if (str == "bodyAddon" && i.isBodyAddon)
						{
							list.Add(i.framePaths);
							continue;
						}
					}
				}
			}

			if (!prop.pawnFrameList.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFrameList i in prop.pawnFrameList)
				{
					if ((i.onlyMale && pawn.gender != Gender.Male) || (i.onlyFemale && pawn.gender != Gender.Female))
					{
						continue;
					}
					if (str == "head" && i.isHead)
					{
						list.Add(i.framePaths);
						continue;
					}
					if (str == "body" && i.isBody)
					{
						list.Add(i.framePaths);
						continue;
					}
					if (str == "hair" && i.isHair)
					{
						list.Add(i.framePaths);
						continue;
					}
				}
			}
			
			return list;
		}

		public List<DAL_CompAnimatedFramePath> loadApparelAnimeFrameList(DAL_CompProperties_Animated prop)
		{
			List<DAL_CompAnimatedFramePath> list = new List<DAL_CompAnimatedFramePath>();

			if (!prop.apparelFrameList.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFrameList i in prop.apparelFrameList)
				{
					if (i.isApparel)
					{
						list.Add(i.framePaths);
					}
				}
			}

			return list;
		}

		public List<DAL_CompAnimatedFramePath> loadThingAnimeFrameList(DAL_CompProperties_Animated prop)
		{
			List<DAL_CompAnimatedFramePath> list = new List<DAL_CompAnimatedFramePath>();

			if (!prop.thingFrameList.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFrameList i in prop.thingFrameList)
				{
					if (i.isThing)
					{
						list.Add(i.framePaths);
					}
				}
			}

			return list;
		}

		public List<DAL_CompAnimatedFramePath> loadCustomAnimeFrameList(DAL_CompProperties_Animated prop)
		{
			List<DAL_CompAnimatedFramePath> list = new List<DAL_CompAnimatedFramePath>();

			if (!prop.customFrameList.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFrameList i in prop.customFrameList)
				{
					if (i.isOverlay)
					{
						list.Add(i.framePaths);
					}
				}
			}
			
			return list;
		}


		//处理帧
		public Dictionary<string, int> DALResolveCurGraphic(ThingWithComps pThingWithComps, DAL_CompProperties_Animated pProps, ref Dictionary<string, int> curIndex, ref int pTicksToCycle, ref bool pDirty, ref List<DAL_GraphicInfo> dalAGR,ref DAL_GraphicInfo dalPrimary, ref List<DAL_GraphicInfo> dalThing, ref List<DAL_GraphicInfo> dalCustom, ref List<DAL_GraphicInfo> headGraphicList, ref List<DAL_GraphicInfo> nakedGraphicList, ref List<DAL_GraphicInfo> hairGraphicList)
		{
			if (pProps.secondsBetweenFrames <= 0f)
			{
				Log.ErrorOnce("DAL_CompAnimated :: DAL_CompProperties_Animated secondsBetweenFrames needs to be more than 0", 132);
			}

			if (pThingWithComps != null && pProps.secondsBetweenFrames > 0f && Find.TickManager.TicksGame > pTicksToCycle)
			{
				pTicksToCycle = Find.TickManager.TicksGame + pProps.secondsBetweenFrames.SecondsToTicks();

				if (!initialization)
				{
					Pawn pawn0 = null;
					bool parentIsPawn = DAL_CompAnimated.AsPawn(pThingWithComps, out pawn);//是人，返回此人
					bool parentParentIsPawn = DAL_CompAnimated.ParentAsPawn(pThingWithComps, out pawn0);//是装备，返回装备的人

					if (pProps.isPawn)
					{
						pawn = parentIsPawn ? pawn : null;
						hasBreastOption = ((pawn as Nazuna_Pawn) != null && (pawn as Nazuna_Pawn).DalDef?.breastSizeList.NullOrEmpty() != true) ? true : false;
						headList = loadPawnAnimeFrameList("head", pProps);
						bodyList = loadPawnAnimeFrameList("body", pProps);
						bodyList.AddRange(loadPawnAnimeFrameList("bodyAddon", pProps));
						hairList = loadPawnAnimeFrameList("hair", pProps);
						hairList.AddRange(loadPawnAnimeFrameList("headAddon", pProps));

					}

					if (pProps.isApparel)
					{
						pawn = parentParentIsPawn ? pawn0 : null;
						hasBreastOption = ((pawn as Nazuna_Pawn) != null && (pawn as Nazuna_Pawn).DalDef?.breastSizeList.NullOrEmpty() != true) ? true : false;
						apparelList = loadApparelAnimeFrameList(pProps);
					}

					if (pProps.isThing)
					{
						pawn = parentParentIsPawn ? pawn0 : null;
						thingList = loadThingAnimeFrameList(pProps);
					}

					customList = loadCustomAnimeFrameList(pProps);

					initialization = true;
				}
				
				

				bool isMoving = pawn?.pather != null ? pawn.pather.MovingNow : false;

				bool isVerbWarmUp = false;
				List<GraphicData> verbFrames = new List<GraphicData>();
				if (pawn != null && !pProps.requireWarmUpVerbs.NullOrEmpty<DAL_AnimateRequireVerb>())
				{
					if (pawn.stances?.curStance?.GetType() == (typeof(Stance_Warmup)))
					{
						isVerbWarmUp = NowAction(pProps.requireWarmUpVerbs, (pawn.stances.curStance as Stance_Warmup).verb, pawn.Rotation, out verbFrames);
					}
				}

				if (isMoving)
				{
					if (pProps.isPawn)
					{
						List<DAL_CompAnimatedFramePath> headFramePaths = new List<DAL_CompAnimatedFramePath>();
						List<DAL_CompAnimatedFramePath> bodyFramePaths = new List<DAL_CompAnimatedFramePath>();
						List<DAL_CompAnimatedFramePath> hairFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!headList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in headList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health.hediffSet.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = headList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (n.bodyPartCustomName != null && (pawn.health?.hediffSet?.PartIsMissing(curAddonBodyParts[n.bodyPartCustomName]) == true || ApparelCoveredBodyAddon(pawn, curAddonBodyParts[n.bodyPartCustomName])))
								{
									continue;
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isMoving)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption && n.headBreastOn)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.headBodySetOn && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								headFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
							}
						}

						if (!bodyList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in bodyList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health.hediffSet.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = bodyList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (n.bodyPartCustomName != null && (pawn.health?.hediffSet?.PartIsMissing(curAddonBodyParts[n.bodyPartCustomName]) == true || ApparelCoveredBodyAddon(pawn, curAddonBodyParts[n.bodyPartCustomName])))
								{
									continue;
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isMoving)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.bodyType != null && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}

								bodyFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}

							}
							/*if (!bodyFramePaths.NullOrEmpty())
							{
								Messages.Message(curIndex[bodyFramePaths.First().name].ToString(), null, MessageTypeDefOf.ThreatBig, false);
							}*/
							//Messages.Message("moving", null, MessageTypeDefOf.ThreatBig, false);
						}

						if (!hairList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in hairList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health.hediffSet.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = hairList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (n.bodyPartCustomName != null && (pawn.health?.hediffSet?.PartIsMissing(curAddonBodyParts[n.bodyPartCustomName]) == true || ApparelCoveredBodyAddon(pawn, curAddonBodyParts[n.bodyPartCustomName])))
								{
									continue;
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isMoving)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption && n.hairBreastOn)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.hairBodySetOn && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}

								hairFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
							}
						}

						if (!headFramePaths.NullOrEmpty() || !bodyFramePaths.NullOrEmpty() || !hairFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							headGraphicList.Clear();
							nakedGraphicList.Clear();
							hairGraphicList.Clear();
							DALResolveCycledGraphic(pawn, curIndex, headFramePaths, bodyFramePaths, hairFramePaths, ref headGraphicList, ref nakedGraphicList, ref hairGraphicList);
						}
					}

					if (pProps.isApparel)
					{
						List<DAL_CompAnimatedFramePath> apparelFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!apparelList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in apparelList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health.hediffSet.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = apparelList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isMoving)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption && n.apparelBreastOn)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.bodyType != null && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								apparelFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
							}
							if (!apparelFramePaths.NullOrEmpty())
							{
								//Messages.Message(curIndex[apparelFramePaths.First().name].ToString(), null, MessageTypeDefOf.ThreatBig, false);
							}
						}

						if (!apparelFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							dalAGR = DALResolveCycledGraphic(pawn, apparelFramePaths, curIndex);
						}

					}

					if (pProps.isThing)
					{
						List<DAL_CompAnimatedFramePath> thingFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!thingList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in thingList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health.hediffSet.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = thingList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isMoving)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/

								if (n.isHediffEffect)
								{
									continue;
								}
								

								thingFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[parent.Rotation];
								}
							}
						}

						if (!thingFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							dalThing.Clear();
							dalThing = DALResolveCycledGraphic(parent, curIndex, thingFramePaths);
							dalPrimary = dalThing.NullOrEmpty() ? null : dalThing[0];
						}
					}

					if (true)
					{
						List<DAL_CompAnimatedFramePath> customFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!customList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in customList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health.hediffSet.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = customList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isMoving)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/

								if (n.isHediffEffect)
								{
									continue;
								}
								

								customFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[parent.Rotation];
								}
							}
						}
						if (!customFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							dalCustom.Clear();
							dalCustom = DALResolveCycledGraphic(parent, curIndex, customFramePaths);
						}
					}
				}
				else
				{
					if (pProps.isPawn)
					{
						List<DAL_CompAnimatedFramePath> headFramePaths = new List<DAL_CompAnimatedFramePath>();
						List<DAL_CompAnimatedFramePath> bodyFramePaths = new List<DAL_CompAnimatedFramePath>();
						List<DAL_CompAnimatedFramePath> hairFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!headList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in headList)
							{
								n = x;
								if (n.hasHediffEffect && pawn.health?.hediffSet?.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = headList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (n.bodyPartCustomName != null && (pawn.health?.hediffSet?.PartIsMissing(curAddonBodyParts[n.bodyPartCustomName]) == true || ApparelCoveredBodyAddon(pawn, curAddonBodyParts[n.bodyPartCustomName])))
								{
									continue;
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isStill)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption && n.headBreastOn)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.headBodySetOn && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								headFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
							}

						}

						if (!bodyList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in bodyList)
							{
								n = x;
								if (n.hasHediffEffect && pawn.health?.hediffSet?.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = bodyList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (n.bodyPartCustomName != null && (pawn.health?.hediffSet?.PartIsMissing(curAddonBodyParts[n.bodyPartCustomName]) == true || ApparelCoveredBodyAddon(pawn, curAddonBodyParts[n.bodyPartCustomName])))
								{
									continue;
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isStill)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.bodyType != null && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								bodyFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
								
							}
							/*if (!bodyFramePaths.NullOrEmpty())
							{
								Messages.Message(curIndex[bodyFramePaths.First().name].ToString(), null, MessageTypeDefOf.ThreatBig, false);
							}*/
							//Messages.Message("stilling", null, MessageTypeDefOf.ThreatBig, false);
						}

						if (!hairList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in hairList)
							{
								n = x;
								if (n.hasHediffEffect && pawn.health?.hediffSet?.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = hairList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (n.bodyPartCustomName != null && (pawn.health?.hediffSet?.PartIsMissing(curAddonBodyParts[n.bodyPartCustomName]) == true || ApparelCoveredBodyAddon(pawn, curAddonBodyParts[n.bodyPartCustomName])))
								{
									continue;
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isStill)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption && n.hairBreastOn)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.hairBodySetOn && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								hairFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
							}
						}

						if (!headFramePaths.NullOrEmpty() || !bodyFramePaths.NullOrEmpty() || !hairFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							headGraphicList.Clear();
							nakedGraphicList.Clear();
							hairGraphicList.Clear();
							DALResolveCycledGraphic(pawn, curIndex, headFramePaths, bodyFramePaths, hairFramePaths, ref headGraphicList, ref nakedGraphicList, ref hairGraphicList);
						}
					}

					if (pProps.isApparel && pawn != null)
					{
						List<DAL_CompAnimatedFramePath> apparelFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!apparelList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in apparelList)
							{
								n = x;
								if (n.hasHediffEffect && pawn.health?.hediffSet?.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = apparelList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (!n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isStill)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && pawn?.Rotation != n.direction)
								{
									continue;
								}*/
								if (hasBreastOption && n.apparelBreastOn)
								{
									if (n.breastSize != (pawn as Nazuna_Pawn).breastSize)
									{
										continue;
									}
								}
								else if (n.bodyType != null && n.bodyType != pawn.story.bodyType)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								apparelFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else 
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[pawn.Rotation];
								}
							}
							if (!apparelFramePaths.NullOrEmpty())
							{
								//Messages.Message(curIndex[apparelFramePaths.First().name].ToString(), null, MessageTypeDefOf.ThreatBig, false);
							}
						}

						if (!apparelFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							dalAGR = DALResolveCycledGraphic(pawn, apparelFramePaths, curIndex);
						}
					}

					if (pProps.isThing)
					{
						List<DAL_CompAnimatedFramePath> thingFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!thingList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in thingList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health?.hediffSet?.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = thingList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (pawn != null && !n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isStill)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && (pawn?.Rotation != n.direction || (pThingWithComps as Building)?.Rotation != n.direction))
								{
									continue;
								}*/
								if (pawn == null && pThingWithComps as Building == null && n.direction != Rot4.South)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								thingFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[parent.Rotation];
								}
							}
						}

						if (!thingFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							dalThing.Clear();
							dalThing = DALResolveCycledGraphic(parent, curIndex, thingFramePaths);
							dalPrimary = dalThing.NullOrEmpty() ? null : dalThing[0];
						}
					}

					if (true)
					{
						List<DAL_CompAnimatedFramePath> customFramePaths = new List<DAL_CompAnimatedFramePath>();
						if (!customList.NullOrEmpty())
						{
							DAL_CompAnimatedFramePath n = null;
							foreach (DAL_CompAnimatedFramePath x in customList)
							{
								n = x;
								if (n.hasHediffEffect && pawn?.health?.hediffSet?.hediffs.Exists(i => n.replaceHediffSet.Exists(a => a.hediffDef == i.def && a.hediffStageIndex == i.CurStageIndex)) == true)
								{
									n = customList.FirstOrDefault(j => j.isHediffEffect && n.replaceHediffSet.FirstOrDefault(a => pawn.health.hediffSet.hediffs.Exists(b => b.def == a.hediffDef && b.CurStageIndex == a.hediffStageIndex)).animeName == j.name);
								}
								if (pawn != null && !n.bodyPartGroups.NullOrEmpty() && ApparelCoverdGraphic(pawn, n))
								{
									continue;
								}
								if (n.thingHasMoveAnime && !n.isStill)
								{
									continue;
								}
								/*if (n.thingHasDirectAnime && (pawn?.Rotation != n.direction || (pThingWithComps as Building)?.Rotation != n.direction))
								{
									continue;
								}*/
								if (pawn == null && pThingWithComps as Building == null && n.thingHasDirectAnime && n.direction != Rot4.South)
								{
									continue;
								}

								if (n.isHediffEffect)
								{
									continue;
								}
								

								customFramePaths.Add(n);
								if (!curIndex.ContainsKey(n.name))
								{
									curIndex.Add(n.name, 0);
								}
								else
								{
									curIndex[n.name] = (curIndex[n.name] + 1) % n.frameCount[parent.Rotation];
								}
							}
						}
						if (!customFramePaths.NullOrEmpty())
						{
							SoundDef sound = pProps.sound;
							if (sound != null)
							{
								sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
							}
							dalCustom.Clear();
							dalCustom = DALResolveCycledGraphic(parent, curIndex, customFramePaths);
						}
					}
				}
				
				if (shouldUpdateHeadFrame)
				{
					UpdateHeadFrameList();
				}
				if (shouldUpdateHairFrame)
				{
					UpdateHairFrameList();
				}
				if (shouldUpdateBodyFrame)
				{
					UpdateBodyFrameList();
				}
				if (shouldUpdateApparelFrame)
				{
					UpdateApparelFrameList();
				}
				if (shouldUpdateThingFrame)
				{
					UpdateThingFrameList();
				}
				if (shouldUpdateCustomFrame)
				{
					UpdateCustomFrameList();
				}

				//旧代码
				if (false)
				{
					/*if (isMoving)
				{
					List<GraphicData> frames = new List<GraphicData>();
					if (pProps.isRot4Moving)
					{
						if ((pawn.Rotation == Rot4.East && !pProps.eastMoveFrames.NullOrEmpty<GraphicData>()) || (pawn.Rotation == Rot4.West && !pProps.eastMoveFrames.NullOrEmpty<GraphicData>()))
						{
							frames = pProps.eastMoveFrames;
						}
						if (pawn.Rotation == Rot4.North && !pProps.northMoveFrames.NullOrEmpty<GraphicData>())
						{
							frames = pProps.northMoveFrames;
						}
						if (pawn.Rotation == Rot4.South && !pProps.southMoveFrames.NullOrEmpty<GraphicData>())
						{
							frames = pProps.southMoveFrames;
						}
					}
					else if (!pProps.movingFrames.NullOrEmpty<GraphicData>())
					{
						frames = pProps.movingFrames;
					}

					if (!frames.NullOrEmpty<GraphicData>())
					{
						pCurIndex = (pCurIndex + 1) % frames.Count<GraphicData>();
						SoundDef sound = pProps.sound;
						if (sound != null)
						{
							sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
						}
						result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, frames);
					}
				}
				else
				{
					List <GraphicData> stillFrames = new List<GraphicData>();
					if (pProps.isRot4Still)
					{
						if (pawn != null)
						{
							if ((pawn.Rotation == Rot4.East && !pProps.eastMoveFrames.NullOrEmpty<GraphicData>()) || (pawn.Rotation == Rot4.West && !pProps.eastMoveFrames.NullOrEmpty<GraphicData>()))
							{
								stillFrames = pProps.eastMoveFrames;
							}
							if (pawn.Rotation == Rot4.North && !pProps.northStillFrames.NullOrEmpty<GraphicData>())
							{
								stillFrames = pProps.northStillFrames;
							}
							if (pawn.Rotation == Rot4.South && !pProps.southStillFrames.NullOrEmpty<GraphicData>())
							{
								stillFrames = pProps.southStillFrames;
							}
						}
						else
						{
							if (pThingWithComps.Rotation == Rot4.East && !pProps.eastStillFrames.NullOrEmpty<GraphicData>())
							{
								stillFrames = pProps.eastStillFrames;
							}
							if (pThingWithComps.Rotation == Rot4.West && !pProps.westStillFrames.NullOrEmpty<GraphicData>())
							{
								stillFrames = pProps.westStillFrames;
							}
							if (pThingWithComps.Rotation == Rot4.North && !pProps.northStillFrames.NullOrEmpty<GraphicData>())
							{
								stillFrames = pProps.northStillFrames;
							}
							if (pThingWithComps.Rotation == Rot4.South && !pProps.southStillFrames.NullOrEmpty<GraphicData>())
							{
								stillFrames = pProps.southStillFrames;
							}
						}
					}
					else if (!pProps.stillFrames.NullOrEmpty<GraphicData>())
					{
						stillFrames = pProps.stillFrames;
					}
					else
					{
						if (pawn != null && useBaseGraphic)//如果是人并且使用基础图像选项为true
						{
							result = DAL_CompAnimated.ResolveBaseGraphic(pawn);
						}
						else if (pawn != null)
						{
							result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false);
						}
						else
						{
							result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, true);
						}
					}

					if (pawn == null)
					{
						pCurIndex = (pCurIndex + 1) % stillFrames.Count<GraphicData>();
						result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, true, null, stillFrames);
					}
					else
					{
						pCurIndex = (pCurIndex + 1) % stillFrames.Count<GraphicData>();
						result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, null, stillFrames);
					}
				}

				if (isVerbWarmUp && !verbFrames.NullOrEmpty<GraphicData>())
				{
					pCurIndex = (pCurIndex + 1) % verbFrames.Count<GraphicData>();
					SoundDef sound = pProps.sound;
					if (sound != null)
					{
						sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
					}
					result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, null, null, verbFrames);
				}*/


					/*bool flag3 = DAL_CompAnimated.AsPawn(pThingWithComps, out pawn);//是人，返回此人
					bool flag31 = DAL_CompAnimated.ParentAsPawn(pThingWithComps, out pawn0);//是装备，返回装备的人*/

					/*bool flag5;
					if (flag3)//如果是人
					{
						bool? flag4;
						if (pawn == null)
						{
							flag4 = null;
						}
						else
						{
							Pawn_PathFollower pather = pawn.pather;
							flag4 = ((pather != null) ? new bool?(pather.MovingNow) : null);//是否在移动中
						}
						flag5 = (flag4 ?? false);
					}
					else//否则
					{
						if (flag31)//如果是装备
						{
							bool? flag4;
							if (pawn0 == null)
							{
								flag4 = null;
							}
							else
							{
								pawn = pawn0;//统一【人】变量
								Pawn_PathFollower pather = pawn.pather;
								flag4 = ((pather != null) ? new bool?(pather.MovingNow) : null);//是否在移动中
							}
							flag5 = (flag4 ?? false);
						}
						else//如果是建筑或其他并且不在移动中
						{
							flag5 = false;
						}
					}*/


					//bool flag6 = flag5;


					//if (flag6)//不论人或装装备的人，如果在移动中（可移动的仅有pawn）
					//{
					//	if (!pProps.movingFrames.NullOrEmpty())
					//	{
					//		List<GraphicData> frames = pProps.movingFrames;
					//		if (pProps.isRot4Moving)
					//		{
					//			if ((pawn.Rotation == Rot4.East && !pProps.eastMoveFrames.NullOrEmpty()) || (pawn.Rotation == Rot4.West && !pProps.eastMoveFrames.NullOrEmpty()))
					//			{
					//				frames = pProps.eastMoveFrames;
					//			}
					//			/*if (pawn.Rotation == Rot4.West && !pProps.westMoveFrames.NullOrEmpty())
					//			{
					//				frames = pProps.westMoveFrames;
					//			}*/
					//			if (pawn.Rotation == Rot4.North && !pProps.northMoveFrames.NullOrEmpty())
					//			{
					//				frames = pProps.northMoveFrames;
					//			}
					//			if (pawn.Rotation == Rot4.South && !pProps.southMoveFrames.NullOrEmpty())
					//			{
					//				frames = pProps.southMoveFrames;
					//			}
					//		}

					//		pCurIndex = (pCurIndex + 1) % frames.Count<GraphicData>();
					//		SoundDef sound = pProps.sound;
					//		if (sound != null)
					//		{
					//			sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
					//		}
					//		result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, frames);
					//	}

					//}
					/*else//如果在静止中（无论是否是人）
					{
						bool flag7 = !pProps.stillFrames.NullOrEmpty<GraphicData>();
						if (flag7)//如果静止动画不为null
						{
							List<GraphicData> stillFrames = pProps.stillFrames;
							if (pProps.isRot4Still)
							{
								if (pawn != null)
								{
									if ((pawn.Rotation == Rot4.East && !pProps.eastMoveFrames.NullOrEmpty()) || (pawn.Rotation == Rot4.West && !pProps.eastMoveFrames.NullOrEmpty()))
									{
										stillFrames = pProps.eastMoveFrames;
									}
									if (pawn.Rotation == Rot4.North && !pProps.northStillFrames.NullOrEmpty())
									{
										stillFrames = pProps.northStillFrames;
									}
									if (pawn.Rotation == Rot4.South && !pProps.southStillFrames.NullOrEmpty())
									{
										stillFrames = pProps.southStillFrames;
									}
								}
								else
								{
									if (pThingWithComps.Rotation == Rot4.East && !pProps.eastStillFrames.NullOrEmpty())
									{
										stillFrames = pProps.eastStillFrames;
									}
									if (pThingWithComps.Rotation == Rot4.West && !pProps.westStillFrames.NullOrEmpty())
									{
										stillFrames = pProps.westStillFrames;
									}
									if (pThingWithComps.Rotation == Rot4.North && !pProps.northStillFrames.NullOrEmpty())
									{
										stillFrames = pProps.northStillFrames;
									}
									if (pThingWithComps.Rotation == Rot4.South && !pProps.southStillFrames.NullOrEmpty())
									{
										stillFrames = pProps.southStillFrames;
									}
								}
							}


							bool flag11;
							List<GraphicData> verbFrames = null;
							bool flag10 = !pProps.requireWarmUpVerbs.NullOrEmpty();
							if (flag10)//如果预热动画不为null
							{
								bool? flag9;
								if (pawn == null)
								{
									flag9 = null;
								}
								else
								{
									Pawn_StanceTracker stanceTracker = pawn.stances;
									if (stanceTracker != null)
									{
										Stance stance = stanceTracker.curStance;
										if (stance.GetType() == typeof(Stance_Warmup))
										{
											flag9 = NowAction(pProps.requireWarmUpVerbs, (stance as Stance_Warmup).verb, pawn.Rotation, out verbFrames);
										}
										else
										{
											flag9 = false;
										}
									}
									else
									{
										flag9 = false;
									}
								}
								flag11 = flag9 ?? false;
							}
							else//如果预热动画为null
							{
								flag11 = false;
							}


							if (flag11 && !verbFrames.NullOrEmpty<GraphicData>())
							{
								pCurIndex = (pCurIndex + 1) % verbFrames.Count<GraphicData>();
								SoundDef sound = pProps.sound;
								if (sound != null)
								{
									sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
								}
								result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, null, null, verbFrames);
								pDirty = false;
								return result;
							}


							if (pawn == null)
							{
								pCurIndex = (pCurIndex + 1) % stillFrames.Count<GraphicData>();
								result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, true, null, stillFrames);
								pDirty = false;
								return result;
							}
							else
							{
								pCurIndex = (pCurIndex + 1) % stillFrames.Count<GraphicData>();
								result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, null, stillFrames);
								pDirty = false;
								return result;
							}
						}
						else//如果静止动画为null
						{
							bool flag11;
							List<GraphicData> verbFrames = null;
							bool flag10 = !pProps.requireWarmUpVerbs.NullOrEmpty();
							if (flag10)
							{
								bool? flag9;
								if (pawn == null)
								{
									flag9 = null;
								}
								else
								{
									Pawn_StanceTracker stanceTracker = pawn.stances;
									if (stanceTracker != null)
									{
										Stance stance = stanceTracker.curStance;
										if (stance.GetType() == typeof(Stance_Warmup))
										{
											flag9 = NowAction(pProps.requireWarmUpVerbs, (stance as Stance_Warmup).verb, pawn.Rotation, out verbFrames);
										}
										else
										{
											flag9 = false;
										}
									}
									else
									{
										flag9 = false;
									}
								}
								flag11 = flag9 ?? false;
							}
							else
							{
								flag11 = false;
							}


							if (flag11 && !verbFrames.NullOrEmpty<GraphicData>())
							{
								pCurIndex = (pCurIndex + 1) % verbFrames.Count<GraphicData>();
								SoundDef sound = pProps.sound;
								if (sound != null)
								{
									sound.PlayOneShot(SoundInfo.InMap(pawn, MaintenanceType.None));
								}
								result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false, null, verbFrames);
								pDirty = false;
								return result;
							}


							bool flag8 = pawn != null && useBaseGraphic;//如果是人并且使用基础图像选项为true
							if (flag8)
							{
								result = DAL_CompAnimated.ResolveBaseGraphic(pawn);
							}
							else
							{
								if (pawn != null)
								{
									result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, false);
								}
								else
								{
									result = DAL_CompAnimated.DALResolveCycledGraphic(pThingWithComps, pProps, pCurIndex, true);
								}
							}
						}
					}*/
				}

			}
			pDirty = false;
			return curIndex;
		}


		//处理图像子方法
		public static DAL_GraphicInfo GetGraphicFromData(DAL_CompAnimatedFramePath data, int index, float yOffSet, string hairDefName = null, float meshSize = -1f, Gender gender = Gender.None)
		{
			DAL_GraphicData n = new DAL_GraphicData(data.frame);
			if (gender != Gender.None)
            {
				n.texPath = Path.Combine(n.texPath, gender.ToString() + "_" + data.name) ;
            }
			n.texPath += index.ToString();
			return new DAL_GraphicInfo(n.Graph, data.isBackGraph, yOffSet, hairDefName, meshSize);
		}


		//处理图像
		public static void DALResolveCycledGraphic(Pawn pawn, Dictionary<string, int> curIndex, List<DAL_CompAnimatedFramePath> head, List<DAL_CompAnimatedFramePath> body, List<DAL_CompAnimatedFramePath> hair, ref List<DAL_GraphicInfo> headGraphicList, ref List<DAL_GraphicInfo> nakedGraphicList, ref List<DAL_GraphicInfo> hairGraphicList, float meshSize = 1.25f)
		{
			if (!head.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFramePath p in head)
				{
					float y = p.defaultYOffSet;
					if (p.ySetFacing != null)
					{
						if (!p.ySetFacing.yRot4Offset.NullOrEmpty())
						{
							y += p.ySetFacing.yRot4Offset[pawn.Rotation];
						}
						if (!p.ySetFacing.yRot4OffsetList.NullOrEmpty())
						{
							foreach (DAL_RendLayerSet r in p.ySetFacing.yRot4OffsetList[pawn.Rotation])
							{
								if (curIndex[p.name] >= r.frameRange.Value.min && curIndex[p.name] <= r.frameRange.Value.max)
								{
									y += r.yOffSet;
									break;
								}
							}
						}
					}
					headGraphicList.Add(GetGraphicFromData(p, curIndex[p.name], y, null, p.meshSize));
				}
			}
			if (!body.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFramePath p in body)
				{
					float y = p.defaultYOffSet;
					if (p.ySetFacing != null)
					{
						if (!p.ySetFacing.yRot4Offset.NullOrEmpty())
						{
							y += p.ySetFacing.yRot4Offset[pawn.Rotation];
						}
						if (!p.ySetFacing.yRot4OffsetList.NullOrEmpty())
						{
							foreach (DAL_RendLayerSet r in p.ySetFacing.yRot4OffsetList[pawn.Rotation])
							{
								if (curIndex[p.name] >= r.frameRange.Value.min && curIndex[p.name] <= r.frameRange.Value.max)
								{
									y += r.yOffSet;
									break;
								}
							}
						}
					}
					nakedGraphicList.Add(GetGraphicFromData(p, curIndex[p.name], y, null, p.meshSize));
				}
				
			}
			if (!hair.NullOrEmpty())
			{
				foreach (DAL_CompAnimatedFramePath p in hair)
				{
					float y = p.defaultYOffSet;
					if (p.ySetFacing != null)
					{
						if (!p.ySetFacing.yRot4Offset.NullOrEmpty())
						{
							y += p.ySetFacing.yRot4Offset[pawn.Rotation];
						}
						if (!p.ySetFacing.yRot4OffsetList.NullOrEmpty())
						{
							foreach (DAL_RendLayerSet r in p.ySetFacing.yRot4OffsetList[pawn.Rotation])
							{
								if (curIndex[p.name] >= r.frameRange.Value.min && curIndex[p.name] <= r.frameRange.Value.max)
								{
									y += r.yOffSet;
									break;
								}
							}
						}
					}
					hairGraphicList.Add(GetGraphicFromData(p, curIndex[p.name], y, p.hairDefName, p.meshSize));
				}
			}
			return;
		}

		public static List<DAL_GraphicInfo> DALResolveCycledGraphic(Pawn pawn, List<DAL_CompAnimatedFramePath> apparel, Dictionary<string, int> curIndex)
		{
			List<DAL_GraphicInfo> graList = new List<DAL_GraphicInfo>();
			foreach (DAL_CompAnimatedFramePath p in apparel)
			{
				float y = p.defaultYOffSet;
				if (p.ySetFacing != null)
				{
					if (!p.ySetFacing.yRot4Offset.NullOrEmpty())
					{
						y += p.ySetFacing.yRot4Offset[pawn.Rotation];
					}
					if (!p.ySetFacing.yRot4OffsetList.NullOrEmpty())
					{
						foreach (DAL_RendLayerSet r in p.ySetFacing.yRot4OffsetList[pawn.Rotation])
						{
							if (curIndex[p.name] >= r.frameRange.Value.min && curIndex[p.name] <= r.frameRange.Value.max)
							{
								y += r.yOffSet;
								break;
							}
						}
					}
				}
				graList.Add(GetGraphicFromData(p, curIndex[p.name], y, null, p.meshSize));
			}
			return graList;
		}

		public static List<DAL_GraphicInfo> DALResolveCycledGraphic(ThingWithComps thing, Dictionary<string, int> curIndex, List<DAL_CompAnimatedFramePath> custom)
		{
			List<DAL_GraphicInfo> graList = new List<DAL_GraphicInfo>();
			foreach (DAL_CompAnimatedFramePath p in custom)
			{
				float y = p.defaultYOffSet;
				if (p.ySetFacing != null)
				{
					if (!p.ySetFacing.yRot4Offset.NullOrEmpty())
					{
						y += p.ySetFacing.yRot4Offset[thing.Rotation];
					}
					if (!p.ySetFacing.yRot4OffsetList.NullOrEmpty())
					{
						foreach (DAL_RendLayerSet r in p.ySetFacing.yRot4OffsetList[thing.Rotation])
						{
							if (curIndex[p.name] >= r.frameRange.Value.min && curIndex[p.name] <= r.frameRange.Value.max)
							{
								y += r.yOffSet;
								break;
							}
						}
					}
				}
				graList.Add(GetGraphicFromData(p, curIndex[p.name], p.defaultYOffSet, null, p.meshSize));
			}
			return graList;
		}

		//旧方法
		public static Graphic DALResolveCycledGraphic(ThingWithComps pAnimatee, DAL_CompProperties_Animated pProps, int pCurIndex, bool isBuilding, List<GraphicData> frames = null, List<GraphicData> stillFrames = null, List<GraphicData> verbFrames = null)
		{
			if (false)
			{
				/*Graphic graphic = null;
				bool flag = !frames.NullOrEmpty<GraphicData>();//移动动画不为null
				bool hasMoveFrame = !frames.NullOrEmpty<GraphicData>();//移动动画不为null

				Pawn pawn;
				Pawn pawn0;
				PawnGraphicSet pawnGraphicSet2;
				List<GraphicData> verbFrames2 = verbFrames;
				bool parentIsPawn = DAL_CompAnimated.AsPawn(pAnimatee, out pawn);//是人，返回此人
				bool parentParentIsPawn = DAL_CompAnimated.ParentAsPawn(pAnimatee, out pawn0);//是装备，返回装备的人


				if (isBuilding)
				{
					if (!stillFrames.NullOrEmpty<GraphicData>())
					{
						graphic = stillFrames[pCurIndex].Graphic;
					}
					else if (hasMoveFrame)
					{
						graphic = pProps.movingFrames[pCurIndex].Graphic;
					}
				}
				else
				{

				}


				if (isBuilding)
				{
					bool flag9 = !stillFrames.NullOrEmpty<GraphicData>();
					if (flag9)
					{
						graphic = stillFrames[pCurIndex].Graphic;
					}
					else
					{
						bool flag10 = flag;
						if (flag10)
						{
							graphic = pProps.movingFrames[pCurIndex].Graphic;
						}
					}
				}
				else
				{
					if (DAL_CompAnimated.AsPawn(pAnimatee, out pawn))//是人，并返回此人
					{
						Pawn pawn1 = pawn ?? null;
						if (pawn1 != null)
						{
							Pawn_DrawTracker drawer = pawn1.Drawer;
							PawnGraphicSet pawnGraphicSet;
							if (drawer == null)
							{
								pawnGraphicSet = null;
							}
							else
							{
								PawnRenderer renderer = drawer.renderer;
								pawnGraphicSet = (renderer != null) ? renderer.graphics : null;
							}
							pawnGraphicSet2 = pawnGraphicSet;
						}
						else
						{
							pawnGraphicSet2 = null;
						}


						if (pawnGraphicSet2 != null)
						{
							Pawn pawn2;
							bool flag2;
							if (flag && DAL_CompAnimated.AsPawn(pAnimatee, out pawn2))
							{
								bool? flag4;
								if (pawn2 == null)
								{
									flag4 = null;
								}
								else
								{
									Pawn_PathFollower pather = pawn2.pather;
									flag4 = ((pather != null) ? new bool?(pather.MovingNow) : null);//是否在移动中
								}
								flag2 = flag4 ?? false;
							}
							else
							{
								flag2 = false;
							}


							if (flag2)
							{
								graphic = frames[pCurIndex].Graphic;

							}
							else
							{
								if (!stillFrames.NullOrEmpty<GraphicData>())
								{
									Pawn pawn3;
									bool flag06;//用于判断是否是预热动作
									if (!verbFrames.NullOrEmpty<GraphicData>() && DAL_CompAnimated.AsPawn(pAnimatee, out pawn3))
									{
										bool? flag05;
										Pawn_StanceTracker stanceTracker = pawn3.stances;
										if (stanceTracker != null)
										{
											Stance stance = stanceTracker.curStance;
											if (stance.GetType() == typeof(Stance_Warmup))
											{
												flag05 = NowAction(pProps.requireWarmUpVerbs, (stance as Stance_Warmup).verb, pawn3.Rotation, out verbFrames);
											}
											else
											{
												flag05 = false;
											}
										}
										else
										{
											flag05 = false;
										}
										flag06 = (flag05 ?? false) && verbFrames == verbFrames2;
									}
									else
									{
										flag06 = false;
									}


									if (flag06)
									{
										graphic = verbFrames[pCurIndex].Graphic;

									}
									else
									{
										graphic = stillFrames[pCurIndex].Graphic;

									}

								}
								else
								{
									Pawn pawn4;
									bool flag06;//用于判断是否是预热动作
									if (!verbFrames.NullOrEmpty<GraphicData>() && DAL_CompAnimated.AsPawn(pAnimatee, out pawn4))
									{
										bool? flag05;
										Pawn_StanceTracker stanceTracker = pawn4.stances;
										if (stanceTracker != null)
										{
											Stance stance = stanceTracker.curStance;
											if (stance.GetType() == typeof(Stance_Warmup))
											{
												flag05 = NowAction(pProps.requireWarmUpVerbs, (stance as Stance_Warmup).verb, pawn4.Rotation, out verbFrames);
											}
											else
											{
												flag05 = false;
											}
										}
										else
										{
											flag05 = false;
										}
										flag06 = (flag05 ?? false) && verbFrames == verbFrames2;
									}
									else
									{
										flag06 = false;
									}
									if (flag06)
									{
										graphic = verbFrames[pCurIndex].Graphic;

									}
									else
									{
										bool flag10 = flag;
										if (flag10)
										{
											graphic = pProps.movingFrames[pCurIndex].Graphic;

										}
									}
								}
							}
						}
						else
						{
							bool flag9 = !pProps.stillFrames.NullOrEmpty<GraphicData>();
							if (flag9)
							{
								graphic = pProps.stillFrames[pCurIndex].Graphic;
							}
							else
							{
								bool flag10 = flag;
								if (flag10)
								{
									graphic = pProps.movingFrames[pCurIndex].Graphic;
								}
							}
						}
					}
					else
					{
						if (DAL_CompAnimated.ParentAsPawn(pAnimatee, out pawn0))//是装备，并返回装备的人
						{
							Pawn pawn2 = pawn0 ?? null;
							if (pawn2 != null)
							{
								Pawn_DrawTracker drawer = pawn2.Drawer;
								PawnGraphicSet pawnGraphicSet;
								if (drawer == null)
								{
									pawnGraphicSet = null;
								}
								else
								{
									PawnRenderer renderer = drawer.renderer;
									pawnGraphicSet = (renderer != null) ? renderer.graphics : null;
								}
								pawnGraphicSet2 = pawnGraphicSet;
							}
							else
							{
								pawnGraphicSet2 = null;
							}


							if (pawnGraphicSet2 != null)
							{
								Pawn pawn5;
								bool flag2;
								if (flag && DAL_CompAnimated.ParentAsPawn(pAnimatee, out pawn5))
								{
									bool? flag4;
									if (pawn5 == null)
									{
										flag4 = null;
									}
									else
									{
										Pawn_PathFollower pather = pawn5.pather;
										flag4 = ((pather != null) ? new bool?(pather.MovingNow) : null);//是否在移动中
									}
									flag2 = flag4 ?? false;
								}
								else
								{
									flag2 = false;
								}

								if (flag2)
								{
									graphic = frames[pCurIndex].Graphic;

								}
								else
								{
									if (!stillFrames.NullOrEmpty<GraphicData>())
									{
										Pawn pawn6;
										bool flag06;//用于判断是否是预热动作
										if (!verbFrames.NullOrEmpty<GraphicData>() && DAL_CompAnimated.ParentAsPawn(pAnimatee, out pawn6))
										{
											bool? flag05;
											Pawn_StanceTracker stanceTracker = pawn6.stances;
											if (stanceTracker != null)
											{
												Stance stance = stanceTracker.curStance;
												if (stance.GetType() == typeof(Stance_Warmup))
												{
													flag05 = NowAction(pProps.requireWarmUpVerbs, (stance as Stance_Warmup).verb, pawn6.Rotation, out verbFrames);
												}
												else
												{
													flag05 = false;
												}
											}
											else
											{
												flag05 = false;
											}
											flag06 = (flag05 ?? false) && verbFrames == verbFrames2;
										}
										else
										{
											flag06 = false;
										}
										if (flag06)
										{
											graphic = verbFrames[pCurIndex].Graphic;

										}
										else
										{
											graphic = stillFrames[pCurIndex].Graphic;

										}

									}
									else
									{
										Pawn pawn7;
										bool flag06;//用于判断是否是预热动作
										if (!verbFrames.NullOrEmpty<GraphicData>() && DAL_CompAnimated.ParentAsPawn(pAnimatee, out pawn7))
										{
											bool? flag05;
											Pawn_StanceTracker stanceTracker = pawn7.stances;
											if (stanceTracker != null)
											{
												Stance stance = stanceTracker.curStance;
												if (stance.GetType() == typeof(Stance_Warmup))
												{
													flag05 = NowAction(pProps.requireWarmUpVerbs, (stance as Stance_Warmup).verb, pawn7.Rotation, out verbFrames);
												}
												else
												{
													flag05 = false;
												}
											}
											else
											{
												flag05 = false;
											}
											flag06 = (flag05 ?? false) && verbFrames == verbFrames2;
										}
										else
										{
											flag06 = false;
										}
										if (flag06)
										{
											graphic = verbFrames[pCurIndex].Graphic;

										}
										else
										{
											bool flag10 = flag;
											if (flag10)
											{
												graphic = pProps.movingFrames[pCurIndex].Graphic;

											}
										}
									}
								}
							}
							else
							{
								bool flag9 = !pProps.stillFrames.NullOrEmpty<GraphicData>();
								if (flag9)
								{
									graphic = pProps.stillFrames[pCurIndex].Graphic;
								}
								else
								{
									bool flag10 = flag;
									if (flag10)
									{
										graphic = pProps.movingFrames[pCurIndex].Graphic;
									}
								}
							}
						}
					}
				}*/


				/*bool flag02;
				if (flag0)
				{
					bool? flag4;
					if (pawn == null)
					{
						flag4 = null;
					}
					else
					{
						flag4 = true;
					}
					flag02 = flag4 ?? false;
				}
				else
				{
					if (flag01)
					{
						bool? flag4;
						if (pawn0 == null)
						{
							flag4 = null;
						}
						else
						{
							pawn = pawn0;//统一【人】变量
							flag4 = true;
						}
						flag02 = flag4 ?? false;
					}
					else
					{
						flag02 = false;
					}
				}


				bool flag5;
				bool flag3 = flag02;
				if (flag3)
				{
					Pawn_DrawTracker drawer = pawn.Drawer;
					PawnGraphicSet pawnGraphicSet;
					if (drawer == null)
					{
						pawnGraphicSet = null;
					}
					else
					{
						PawnRenderer renderer = drawer.renderer;
						pawnGraphicSet = (renderer != null) ? renderer.graphics : null;
					}
					flag5 = ((pawnGraphicSet2 = pawnGraphicSet) != null);
				}
				else
				{
					flag5 = false;
				}


				if (flag5)//判断是否是pawn
				{


					bool flag2;
					if (flag)
					{
						bool? flag4;
						if (pawn == null)
						{
							flag4 = null;
						}
						else
						{
							Pawn_PathFollower pather = pawn.pather;
							flag4 = ((pather != null) ? new bool?(pather.MovingNow) : null);//是否在移动中
						}
						flag2 = flag4 ?? false;
					}
					else
					{
						flag2 = false;
					}


					bool flag6 = flag2;//判断是都在移动中（仅有pawn可移动）

					if (flag6 && flag0 && !flag01)
					{
						pawnGraphicSet2.ClearCache();
						graphic = frames[pCurIndex].Graphic;
						pawnGraphicSet2.nakedGraphic = graphic;
					}
					else
					{
						if (flag6 && !flag0 && flag01)
						{
							graphic = frames[pCurIndex].Graphic;
						}
						else
						{

							if (!flag6)
							{
								bool flag03 = !stillFrames.NullOrEmpty<GraphicData>();
								if (flag03)
								{

									bool flag06;//用于判断是否是预热动作

									bool flag04 = !verbFrames.NullOrEmpty<GraphicData>();
									if (flag04 && pawn != null)
									{
										bool? flag05;
										if (pawn == null)
										{
											flag05 = null;
										}
										else
										{
											VerbTracker vbtracker = pawn.VerbTracker;
											flag05 = ((vbtracker != null) ? new bool?(vbtracker.PrimaryVerb == pProps.requireVerb && vbtracker.PrimaryVerb.WarmingUp) : null);
										}
										flag06 = flag05 ?? false;
									}
									else
									{
										flag06 = false;
									}


									if (flag06 && flag0 && !flag01)
									{
										graphic = verbFrames[pCurIndex].Graphic;
										pawnGraphicSet2.nakedGraphic = graphic;
									}
									else
									{
										if (flag06 && !flag0 && flag01)
										{
											graphic = verbFrames[pCurIndex].Graphic;
										}
										else
										{
											if (!flag06 && flag0 && !flag01)
											{
												graphic = stillFrames[pCurIndex].Graphic;
												pawnGraphicSet2.nakedGraphic = graphic;
											}
											else
											{
												if (!flag06 && !flag0 && flag01)
												{
													graphic = stillFrames[pCurIndex].Graphic;
												}
												else
												{
													graphic = null;
												}
											}
										}
									}

								}
								else
								{
									bool flag06;//用于判断是否是预热动作

									bool flag04 = !verbFrames.NullOrEmpty<GraphicData>();
									if (flag04 && pawn != null)
									{
										bool? flag05;
										if (pawn == null)
										{
											flag05 = null;
										}
										else
										{
											VerbTracker vbtracker = pawn.VerbTracker;
											flag05 = ((vbtracker != null) ? new bool?(vbtracker.PrimaryVerb == pProps.requireVerb && vbtracker.PrimaryVerb.WarmingUp) : null);
										}
										flag06 = flag05 ?? false;
									}
									else
									{
										flag06 = false;
									}


									if (flag06 && flag0 && !flag01)
									{
										graphic = verbFrames[pCurIndex].Graphic;
										pawnGraphicSet2.nakedGraphic = graphic;
									}
									else
									{
										if (flag06 && !flag0 && flag01)
										{
											graphic = verbFrames[pCurIndex].Graphic;
										}
									}
								}
							}
						}
					}
				}
				else
				{
					bool flag9 = !pProps.stillFrames.NullOrEmpty<GraphicData>();
					if (flag9)
					{
						graphic = pProps.stillFrames[pCurIndex].Graphic;
					}
					else
					{
						bool flag10 = flag;
						if (flag10)
						{
							graphic = pProps.movingFrames[pCurIndex].Graphic;
						}
					}
				}*/
			}

			return null;
		}

		//处理动态
		public void DALResolveGameObject()
		{
			if (DALProps.objectPlan == null)
			{
				return;
			}
			if (curGameObject == null)
			{
				curGameObject = UnityEngine.Object.Instantiate(new GameObject(), this.parent.DrawPos, Quaternion.Euler(90f, 0f, 0f));
			}
			curGameObject.transform.position = this.parent.DrawPos;
			if (!subGameObjectInitialized)
			{
				foreach (DAL_GameObjectPlanData data in DALProps.objectPlan.dataList)
				{
					GameObject obj = DAL_WorldCurrent.GOM.SetupGameObject(DALProps.objectPlan.modId, data, curGameObject.transform);
					subGameObjectArray.Add(data.name, obj);
				}
				subGameObjectInitialized = true;
			}
		}



		public DAL_CompProperties_Animated DALProps
		{
			get
			{
				return (DAL_CompProperties_Animated)this.props;
			}
		}

		public virtual void NotifyGraphChange()
		{
		}
	}
}
