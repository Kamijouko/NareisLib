using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using AlienRace.ExtendedGraphics;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace NareisLib
{
	[StaticConstructorOnStartup]
	public class Building_NewOutfitStand : Building, IThingHolderEvents<Thing>, IStorageGroupMember, IHaulEnroute, ILoadReferenceable, IHaulDestination, IStoreSettingsParent, IHaulSource, IThingHolder, IApparelSource, ISearchableContents, IBeautyContainer
	{
		private static void InitGraphics()
		{
			if (Building_NewOutfitStand.initializedTextures)
			{
				return;
			}
			Building_NewOutfitStand.initializedTextures = true;
			Building_NewOutfitStand.baseGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("Things/Building/OutfitStand/OutfitStand_Base", ShaderDatabase.Cutout, Building_NewOutfitStand.baseDrawSize, Color.white);
			Building_NewOutfitStand.bodyGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("Things/Building/OutfitStand/OutfitStand_Body", ShaderDatabase.Cutout, Building_NewOutfitStand.bodyDrawSize, Color.white);
			Building_NewOutfitStand.bodyGraphicChild = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("Things/Building/OutfitStand/OutfitStand_BodyChild", ShaderDatabase.Cutout, Building_NewOutfitStand.bodyChildDrawSize, Color.white);
			Building_NewOutfitStand.headGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("Things/Building/OutfitStand/OutfitStand_Head", ShaderDatabase.Cutout, Building_NewOutfitStand.headDrawSize, Color.white);
			Building_NewOutfitStand.swapOutfitIcon = ContentFinder<Texture2D>.Get("UI/Commands/SwapOutfits", true);
		}

		// (get) Token: 0x0600EAC7 RID: 60103 RVA: 0x00444AF7 File Offset: 0x00442CF7
		// (set) Token: 0x0600EAC8 RID: 60104 RVA: 0x00444AFF File Offset: 0x00442CFF
		StorageGroup IStorageGroupMember.Group
		{
			get
			{
				return this.storageGroup;
			}
			set
			{
				this.storageGroup = value;
			}
		}

		// (get) Token: 0x0600EAC9 RID: 60105 RVA: 0x00444B08 File Offset: 0x00442D08
		public StorageSettings StoreSettings
		{
			get
			{
				StorageGroup storageGroup = this.storageGroup;
				return ((storageGroup != null) ? storageGroup.GetStoreSettings() : null) ?? this.settings;
			}
		}

		// (get) Token: 0x0600EACA RID: 60106 RVA: 0x00443A4D File Offset: 0x00441C4D
		StorageSettings IStorageGroupMember.ParentStoreSettings
		{
			get
			{
				return this.def.building.fixedStorageSettings;
			}
		}

		// (get) Token: 0x0600EACB RID: 60107 RVA: 0x00444B26 File Offset: 0x00442D26
		StorageSettings IStorageGroupMember.ThingStoreSettings
		{
			get
			{
				return this.settings;
			}
		}

		// (get) Token: 0x0600EACC RID: 60108 RVA: 0x00443B0F File Offset: 0x00441D0F
		string IStorageGroupMember.StorageGroupTag
		{
			get
			{
				return this.def.building.storageGroupTag;
			}
		}

		// (get) Token: 0x0600EACD RID: 60109 RVA: 0x00443AFF File Offset: 0x00441CFF
		bool IStorageGroupMember.DrawConnectionOverlay
		{
			get
			{
				return base.Spawned;
			}
		}

		// (get) Token: 0x0600EACE RID: 60110 RVA: 0x000028E7 File Offset: 0x00000AE7
		bool IStorageGroupMember.DrawStorageTab
		{
			get
			{
				return true;
			}
		}

		// (get) Token: 0x0600EACF RID: 60111 RVA: 0x0043BA4F File Offset: 0x00439C4F
		bool IStorageGroupMember.ShowRenameButton
		{
			get
			{
				return base.Faction == Faction.OfPlayer;
			}
		}

		// (get) Token: 0x0600EAD0 RID: 60112 RVA: 0x000028E7 File Offset: 0x00000AE7
		public bool StorageTabVisible
		{
			get
			{
				return true;
			}
		}

		public StorageSettings GetStoreSettings()
		{
			StorageGroup storageGroup = this.storageGroup;
			return ((storageGroup != null) ? storageGroup.GetStoreSettings() : null) ?? this.settings;
		}

		StorageSettings IStoreSettingsParent.GetParentStoreSettings()
		{
			return this.def.building.fixedStorageSettings;
		}

		void IStoreSettingsParent.Notify_SettingsChanged()
		{
			if (!base.Spawned)
			{
				return;
			}
			base.MapHeld.listerHaulables.Notify_HaulSourceChanged(this);
		}

		int IHaulEnroute.SpaceRemainingFor(ThingDef def)
		{
			if (def.IsApparel)
			{
				if (!this.GetStoreSettings().AllowedToAccept(def))
				{
					return 0;
				}
				if (!this.HasRoomForApparelOfDef(def))
				{
					return 0;
				}
				return 1;
			}
			else
			{
				if (!def.IsWeapon)
				{
					return 0;
				}
				if (this.holdingWeaponCached)
				{
					return 0;
				}
				if (!this.GetStoreSettings().AllowedToAccept(def))
				{
					return 0;
				}
				return 1;
			}
		}

		bool IHaulDestination.Accepts(Thing t)
		{
			if (t is Apparel)
			{
				return this.GetStoreSettings().AllowedToAccept(t) && (this.innerContainer.Contains(t) || this.HasRoomForApparelOfDef(t.def));
			}
			return t.def.IsWeapon && this.GetStoreSettings().AllowedToAccept(t) && (!this.holdingWeaponCached || this.innerContainer.Contains(t));
		}

		// (get) Token: 0x0600EAD6 RID: 60118 RVA: 0x000028E7 File Offset: 0x00000AE7
		bool IHaulDestination.HaulDestinationEnabled
		{
			get
			{
				return true;
			}
		}

		// (get) Token: 0x0600EAD7 RID: 60119 RVA: 0x00444BFF File Offset: 0x00442DFF
		bool IHaulSource.HaulSourceEnabled
		{
			get
			{
				return this.allowRemovingItems;
			}
		}

		// (get) Token: 0x0600EAD8 RID: 60120 RVA: 0x00444BFF File Offset: 0x00442DFF
		bool IApparelSource.ApparelSourceEnabled
		{
			get
			{
				return this.allowRemovingItems;
			}
		}

		void IThingHolder.GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.innerContainer);
		}

		ThingOwner IThingHolder.GetDirectlyHeldThings()
		{
			return this.innerContainer;
		}

		// (get) Token: 0x0600EADB RID: 60123 RVA: 0x00444C15 File Offset: 0x00442E15
		ThingOwner ISearchableContents.SearchableContents
		{
			get
			{
				return this.innerContainer;
			}
		}

		// (get) Token: 0x0600EADC RID: 60124 RVA: 0x00444C20 File Offset: 0x00442E20
		public float BeautyOffset
		{
			get
			{
				if (this.cachedBeauty < 0f)
				{
					bool outdoors = this.GetRoom(RegionType.Set_All).PsychologicallyOutdoors;
					this.cachedBeauty = this.HeldItems.Aggregate(0f, (float beauty, Thing apparel) => beauty + apparel.GetBeauty(outdoors));
				}
				return this.cachedBeauty - 0.001f;
			}
		}

		public void Notify_ItemAdded(Thing thing)
		{
			this.cachedBeauty = -1f;
			if (base.Spawned)
			{
				base.MapHeld.listerHaulables.Notify_AddedThing(thing);
				this.DirtyRoomStats();
			}
			if (!(thing is Apparel) && thing.def.IsWeapon)
			{
				this.holdingWeaponCached = true;
			}
			this.RecacheGraphics();
		}

		public void Notify_ItemRemoved(Thing thing)
		{
			this.cachedBeauty = -1f;
			if (base.Spawned)
			{
				base.MapHeld.listerHaulables.Notify_DeSpawned(thing);
				this.DirtyRoomStats();
			}
			if (this.holdingWeaponCached && !(thing is Apparel) && thing.def.IsWeapon)
			{
				this.holdingWeaponCached = false;
			}
			this.RecacheGraphics();
		}

		// (get) Token: 0x0600EADF RID: 60127 RVA: 0x00444D41 File Offset: 0x00442F41
		string IBeautyContainer.BeautyOffsetExplanation
		{
			get
			{
				return string.Format("{0}: {1}", "ContainedApparelBeauty".Translate(), this.BeautyOffset.ToStringWithSign("0.##"));
			}
		}

		// (get) Token: 0x0600EAE0 RID: 60128 RVA: 0x00444D6C File Offset: 0x00442F6C
		protected virtual BodyTypeDef BodyTypeDefForRendering
		{
			get
			{
				return GetSelectedBodyTypeDef();
			}
		}

		// (get) Token: 0x0600EAE1 RID: 60129 RVA: 0x0005013E File Offset: 0x0004E33E
		protected virtual float WeaponDrawDistanceFactor
		{
			get
			{
				return 1f;
			}
		}

		// (get) Token: 0x0600EAE2 RID: 60130 RVA: 0x00444D74 File Offset: 0x00442F74
		public ThingWithComps HeldWeapon
		{
			get
			{
				if (!this.holdingWeaponCached)
				{
					return null;
				}
				return this.innerContainer.InnerListForReading.FirstOrDefault((Thing t) => t.def.IsWeapon) as ThingWithComps;
			}
		}

		public Building_NewOutfitStand()
		{
			this.innerContainer = new ThingOwner<Thing>(this);
		}

		// (get) Token: 0x0600EAE4 RID: 60132 RVA: 0x00444DF4 File Offset: 0x00442FF4
		public IReadOnlyList<Thing> HeldItems
		{
			get
			{
				return this.innerContainer.InnerListForReading;
			}
		}

		public bool AddApparel(Apparel apparel)
		{
			return this.innerContainer.TryAdd(apparel, true);
		}

		public bool RemoveApparel(Apparel apparel)
		{
			return this.innerContainer.Remove(apparel);
		}

		public bool RemoveHeldWeapon(Thing weapon)
		{
			return this.holdingWeaponCached && this.HeldWeapon == weapon && this.innerContainer.Remove(weapon);
		}

		public bool TryAddHeldWeapon(Thing weapon)
		{
			if (this.holdingWeaponCached)
			{
				return false;
			}
			if (!this.innerContainer.TryAdd(weapon, true))
			{
				return false;
			}
			this.holdingWeaponCached = true;
			return true;
		}

		public bool TryDrop(Thing thing, IntVec3 cell, ThingPlaceMode mode, int count, out Thing dropped)
		{
			if (this.innerContainer.TryDrop(thing, cell, base.Map, mode, count, out dropped, null, null))
			{
				return true;
			}
			dropped = null;
			return false;
		}

		private void DirtyRoomStats()
		{
			Room room = this.GetRoom(RegionType.Set_All);
			if (room == null)
			{
				return;
			}
			room.Notify_ContainedThingSpawnedOrDespawned(this);
		}

		public bool CanEverStoreThing(Thing t)
		{
			return this.def.building.fixedStorageSettings.AllowedToAccept(t);
		}

		public bool HasRoomForApparelOfDef(ThingDef t)
		{
			foreach (Thing thing in this.innerContainer)
			{
				Apparel apparel = thing as Apparel;
				if (apparel != null && !ApparelUtility.CanWearTogether(t, apparel.def, BodyDefOf.Human))
				{
					return false;
				}
			}
			return true;
		}

		public bool TryDropThingsToMakeRoomForThingOfDef(ThingDef t)
		{
			if (this.HasRoomForApparelOfDef(t))
			{
				return true;
			}
			List<Apparel> list = (from ap in this.innerContainer.InnerListForReading.OfType<Apparel>()
				where !ApparelUtility.CanWearTogether(t, ap.def, BodyDefOf.Human)
				select ap).ToList<Apparel>();
			if (list.NullOrEmpty<Apparel>())
			{
				return true;
			}
			foreach (Apparel apparel in list)
			{
				Thing thing;
				if (!this.innerContainer.TryDrop(apparel, ThingPlaceMode.Near, out thing, null, null))
				{
					return false;
				}
			}
			return true;
		}

		public override void PostMake()
		{
			base.PostMake();
			this.settings = new StorageSettings(this);
			if (this.def.building.defaultStorageSettings != null)
			{
				this.settings.CopyFrom(this.def.building.defaultStorageSettings);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (this.storageGroup != null && map != this.storageGroup.Map)
			{
				StorageSettings storeSettings = this.storageGroup.GetStoreSettings();
				this.storageGroup.RemoveMember(this, true);
				this.storageGroup = null;
				this.settings.CopyFrom(storeSettings);
			}
			LongEventHandler.ExecuteWhenFinished(new Action(this.RecacheGraphics));
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != DestroyMode.WillReplace && mode != DestroyMode.Vanish)
			{
				this.innerContainer.TryDropAll(base.PositionHeld, base.MapHeld, ThingPlaceMode.Near, null, null, true);
			}
			base.DeSpawn(mode);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			if (this.storageGroup != null)
			{
				StorageGroup storageGroup = this.storageGroup;
				if (storageGroup != null)
				{
					storageGroup.RemoveMember(this, true);
				}
				this.storageGroup = null;
			}
		}

		public override void Notify_MinifiedThingAboutToBeDestroyed(DestroyMode mode)
		{
			if (mode != DestroyMode.WillReplace && mode != DestroyMode.Vanish)
			{
				this.innerContainer.TryDropAll(base.PositionHeld, base.MapHeld, ThingPlaceMode.Near, null, null, true);
			}
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (Find.Selector.SingleSelectedThing == this)
			{
				Room room = this.GetRoom(RegionType.Set_All);
				if (room != null && room.ProperRoom)
				{
					room.DrawFieldEdges();
				}
			}
			StorageGroupUtility.DrawSelectionOverlaysFor(this);
		}

		private void RecacheGraphics()
		{
			this.cachedApparelGraphicsHeadgear.Clear();
			this.cachedApparelGraphicsNonHeadgear.Clear();
			this.cachedMultiTexApparelHeadgear.Clear();
			this.cachedMultiTexApparelNonHeadgear.Clear();
			this.cachedMultiTexStandBody.Clear();
			this.cachedMultiTexStandHead.Clear();
			this.useMultiTexBody = false;
			this.useMultiTexHead = false;
			this.hideOriginalBody = false;
			this.hideOriginalHead = false;
			this.cachedApparelRenderInfoSkipHead = false;
			this.standBodyGraphicOverride = null;
			this.standHeadGraphicOverride = null;
			RenderTreeLayout renderTreeLayout = GetRenderTreeLayout();
			DummyExtendedGraphicsPawnWrapper previousDummy = AlienPartGenerator.ExtendedGraphicTop.drawOverrideDummy;
			DummyExtendedGraphicsPawnWrapper dummy = BuildRenderDummy();
			AlienPartGenerator.ExtendedGraphicTop.drawOverrideDummy = dummy;
			try
			{
				CacheStandMultiTex();
				UpdateOriginalBodyHeadVisibility();
				UpdateAlienRaceGraphicOverrides();
				List<Apparel> apparelList = this.innerContainer.InnerListForReading.OfType<Apparel>().ToList<Apparel>();
				apparelList.SortBy((Apparel a) => a.def.apparel.LastLayer.drawOrder);
				Dictionary<PawnRenderNodeTagDef, int> layerOffsets = new Dictionary<PawnRenderNodeTagDef, int>();
				foreach (Apparel apparel in apparelList)
				{
					ApparelLayerDef lastLayer = apparel.def.apparel.LastLayer;
					bool isHeadgear = lastLayer == ApparelLayerDefOf.Overhead || lastLayer == ApparelLayerDefOf.EyeCover;
					PawnRenderNodeTagDef parentTag = apparel.def.apparel.parentTagDef;
					if (parentTag != null)
					{
						if (parentTag == PawnRenderNodeTagDefOf.ApparelHead)
						{
							isHeadgear = true;
						}
						else if (parentTag == PawnRenderNodeTagDefOf.ApparelBody)
						{
							isHeadgear = false;
						}
					}
					if (parentTag == null)
					{
						parentTag = isHeadgear ? PawnRenderNodeTagDefOf.ApparelHead : PawnRenderNodeTagDefOf.ApparelBody;
					}
					this.cachedApparelRenderInfoSkipHead = this.cachedApparelRenderInfoSkipHead || apparel.def.apparel.renderSkipFlags.NotNullAndContains(RenderSkipFlagDefOf.Head);
					int layerIndex = layerOffsets.TryGetValue(parentTag, 0);
					layerOffsets[parentTag] = layerIndex + 1;
					float parentBaseLayer = GetNodeBaseLayer(renderTreeLayout, parentTag);
					float absoluteBaseLayer = parentBaseLayer + layerIndex;
					float layer = ResolveVanillaApparelLayer(apparel, base.Rotation, absoluteBaseLayer) - parentBaseLayer;
					Vector3 scale = Vector3.one;
					Vector3 positionOffset = Vector3.zero;
					if (apparel.RenderAsPack())
					{
						Vector2 beltScale = apparel.def.apparel.wornGraphicData.BeltScaleAt(base.Rotation, this.BodyTypeDefForRendering);
						scale.x *= beltScale.x;
						scale.z *= beltScale.y;
						Vector2 beltOffset = apparel.def.apparel.wornGraphicData.BeltOffsetAt(base.Rotation, this.BodyTypeDefForRendering);
						positionOffset.x += beltOffset.x;
						positionOffset.z += beltOffset.y;
					}

					MultiTexDef multiTexDef;
					bool hasMultiTexDef = TryGetMultiTexDef(apparel, out multiTexDef);
					if (hasMultiTexDef)
					{
						bool cachedMulti = TryCacheMultiTexApparel(apparel, multiTexDef, 0f, scale, positionOffset);
						if (cachedMulti || multiTexDef != null)
						{
							continue;
						}
					}

					ApparelGraphicRecord apparelGraphicRecord;
					if (ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, this.BodyTypeDefForRendering, false, out apparelGraphicRecord))
					{
						if (isHeadgear)
						{
							this.cachedApparelGraphicsHeadgear.Add(new Building_NewOutfitStand.CachedGraphicRenderInfo(apparelGraphicRecord.graphic, layer, scale, positionOffset, true));
						}
						else
						{
							this.cachedApparelGraphicsNonHeadgear.Add(new Building_NewOutfitStand.CachedGraphicRenderInfo(apparelGraphicRecord.graphic, layer, scale, positionOffset));
						}
					}
				}
			}
			finally
			{
				AlienPartGenerator.ExtendedGraphicTop.drawOverrideDummy = previousDummy;
			}

			if (this.cachedApparelGraphicsHeadgear.Count > 1)
			{
				this.cachedApparelGraphicsHeadgear.Sort((Building_NewOutfitStand.CachedGraphicRenderInfo a, Building_NewOutfitStand.CachedGraphicRenderInfo b) => a.layer.CompareTo(b.layer));
			}
			if (this.cachedApparelGraphicsNonHeadgear.Count > 1)
			{
				this.cachedApparelGraphicsNonHeadgear.Sort((Building_NewOutfitStand.CachedGraphicRenderInfo a, Building_NewOutfitStand.CachedGraphicRenderInfo b) => a.layer.CompareTo(b.layer));
			}
			if (this.cachedMultiTexApparelHeadgear.Count > 1)
			{
				this.cachedMultiTexApparelHeadgear.Sort((Building_NewOutfitStand.CachedMultiTexRenderInfo a, Building_NewOutfitStand.CachedMultiTexRenderInfo b) => a.layer.CompareTo(b.layer));
			}
			if (this.cachedMultiTexApparelNonHeadgear.Count > 1)
			{
				this.cachedMultiTexApparelNonHeadgear.Sort((Building_NewOutfitStand.CachedMultiTexRenderInfo a, Building_NewOutfitStand.CachedMultiTexRenderInfo b) => a.layer.CompareTo(b.layer));
			}
			if (this.cachedMultiTexStandBody.Count > 1)
			{
				this.cachedMultiTexStandBody.Sort((Building_NewOutfitStand.CachedMultiTexRenderInfo a, Building_NewOutfitStand.CachedMultiTexRenderInfo b) => a.layer.CompareTo(b.layer));
			}
			if (this.cachedMultiTexStandHead.Count > 1)
			{
				this.cachedMultiTexStandHead.Sort((Building_NewOutfitStand.CachedMultiTexRenderInfo a, Building_NewOutfitStand.CachedMultiTexRenderInfo b) => a.layer.CompareTo(b.layer));
			}
		}

		private bool TryGetMultiTexDef(Apparel apparel, out MultiTexDef def)
		{
			def = null;
			if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
			{
				return false;
			}
			string appFullOriginalDefName = string.Concat(new string[]
			{
				typeof(ThingDef).ToStringSafe(),
				"_",
				apparel.def.defName
			});
			string renderPlanDef = EffectiveRenderPlanDefName;
			Dictionary<string, MultiTexDef> planDefs;
			if (!renderPlanDef.NullOrEmpty() && ThisModData.DefAndKeyDatabase.TryGetValue(renderPlanDef, out planDefs))
			{
				if (planDefs.TryGetValue(appFullOriginalDefName, out def))
				{
					return true;
				}
			}
			foreach (KeyValuePair<string, Dictionary<string, MultiTexDef>> pair in ThisModData.DefAndKeyDatabase)
			{
				if (pair.Value != null && pair.Value.TryGetValue(appFullOriginalDefName, out def))
				{
					return true;
				}
			}
			return false;
		}

		private void CacheStandMultiTex()
		{
			string planName = EffectiveRenderPlanDefName;
			if (planName.NullOrEmpty())
			{
				return;
			}
			Dictionary<string, MultiTexDef> plan;
			if (!ThisModData.DefAndKeyDatabase.TryGetValue(planName, out plan) || plan.NullOrEmpty())
			{
				return;
			}
			BodyTypeDef bodyDef = GetSelectedBodyTypeDef();
			HeadTypeDef headDef = GetSelectedHeadTypeDef();
			CacheStandMultiTexFor(plan, typeof(BodyTypeDef), bodyDef != null ? bodyDef.defName : "", this.cachedMultiTexStandBody, this.cachedMultiTexStandHead);
			CacheStandMultiTexFor(plan, typeof(HeadTypeDef), headDef != null ? headDef.defName : "", this.cachedMultiTexStandBody, this.cachedMultiTexStandHead);
		}

		private void UpdateOriginalBodyHeadVisibility()
		{
			this.hideOriginalBody = false;
			this.hideOriginalHead = false;
			string planName = EffectiveRenderPlanDefName;
			if (planName.NullOrEmpty())
			{
				return;
			}
			Dictionary<string, MultiTexDef> plan;
			if (!ThisModData.DefAndKeyDatabase.TryGetValue(planName, out plan) || plan.NullOrEmpty())
			{
				return;
			}
			BodyTypeDef bodyDef = GetSelectedBodyTypeDef();
			MultiTexDef def;
			if (bodyDef != null && ((TryGetPlanMultiTexDef(plan, typeof(BodyTypeDef), bodyDef.defName, out def) || TryFindMultiTexDef(typeof(BodyTypeDef), bodyDef.defName, out def)) && def != null))
			{
				this.hideOriginalBody = !def.renderOriginTex;
			}
			HeadTypeDef headDef = GetSelectedHeadTypeDef();
			if (headDef != null && ((TryGetPlanMultiTexDef(plan, typeof(HeadTypeDef), headDef.defName, out def) || TryFindMultiTexDef(typeof(HeadTypeDef), headDef.defName, out def)) && def != null))
			{
				this.hideOriginalHead = !def.renderOriginTex;
			}
		}

		private void CacheStandMultiTexFor(Dictionary<string, MultiTexDef> plan, Type defClass, string defName, List<Building_NewOutfitStand.CachedMultiTexRenderInfo> bodyList, List<Building_NewOutfitStand.CachedMultiTexRenderInfo> headList)
		{
			if (plan.NullOrEmpty() || defName.NullOrEmpty())
			{
				return;
			}
			string key = defClass.ToStringSafe() + "_" + defName;
			MultiTexDef def;
			if (!plan.TryGetValue(key, out def) || def == null)
			{
				if (!TryFindMultiTexDef(defClass, defName, out def))
				{
					return;
				}
			}
			Dictionary<string, TextureLevels> data;
			MultiTexEpoch epoch;
			List<TextureLevels> actionList = new List<TextureLevels>();
			List<ActionManager> actionManagers = new List<ActionManager>();
			try
			{
				epoch = MultiRenderComp.ResolveMultiTexDef(def, out data, actionList, actionManagers, null);
			}
			catch
			{
				return;
			}
			if (epoch == null || epoch.batches.NullOrEmpty() || data.NullOrEmpty())
			{
				return;
			}
			foreach (MultiTexBatch batch in epoch.batches)
			{
				TextureLevels level;
				if (!data.TryGetValue(batch.textureLevelsName, out level))
				{
					continue;
				}
				float layer = ResolveLayerForLevel(level, base.Rotation, 0f);
				bool useHeadOffset = IsHeadLayer(level.renderLayer) || level.offsetToHead;
				Vector3 scale = Vector3.one;
				Building_NewOutfitStand.CachedMultiTexRenderInfo item = new Building_NewOutfitStand.CachedMultiTexRenderInfo(level, batch, null, layer, scale, Vector3.zero, useHeadOffset);
				if (useHeadOffset)
				{
					headList.Add(item);
					useMultiTexHead = true;
				}
				else
				{
					bodyList.Add(item);
					useMultiTexBody = true;
				}
			}
		}

		private bool TryFindMultiTexDef(Type defClass, string defName, out MultiTexDef def)
		{
			def = null;
			if (ThisModData.DefAndKeyDatabase.NullOrEmpty() || defName.NullOrEmpty())
			{
				return false;
			}
			string key = defClass.ToStringSafe() + "_" + defName;
			foreach (KeyValuePair<string, Dictionary<string, MultiTexDef>> pair in ThisModData.DefAndKeyDatabase)
			{
				if (pair.Value != null && pair.Value.TryGetValue(key, out def))
				{
					return true;
				}
			}
			return false;
		}

		private static bool TryGetPlanMultiTexDef(Dictionary<string, MultiTexDef> plan, Type defClass, string defName, out MultiTexDef def)
		{
			def = null;
			if (plan.NullOrEmpty() || defName.NullOrEmpty())
			{
				return false;
			}
			string key = defClass.ToStringSafe() + "_" + defName;
			if (plan.TryGetValue(key, out def) && def != null)
			{
				return true;
			}
			return false;
		}

		private bool TryCacheMultiTexApparel(Apparel apparel, MultiTexDef def, float baseLayer, Vector3 scale, Vector3 positionOffset)
		{
			if (def == null)
			{
				return false;
			}
			Dictionary<string, TextureLevels> data;
			MultiTexEpoch multiTexEpoch;
			List<TextureLevels> actionList = new List<TextureLevels>();
			List<ActionManager> actionManagers = new List<ActionManager>();
			Rand.PushState();
			Rand.Seed = apparel.thingIDNumber;
			try
			{
				multiTexEpoch = MultiRenderComp.ResolveMultiTexDef(def, out data, actionList, actionManagers, apparel);
			}
			finally
			{
				Rand.PopState();
			}
			if (multiTexEpoch == null || multiTexEpoch.batches.NullOrEmpty() || data.NullOrEmpty())
			{
				return false;
			}
			foreach (MultiTexBatch multiTexBatch in multiTexEpoch.batches)
			{
				TextureLevels textureLevels;
				if (!data.TryGetValue(multiTexBatch.textureLevelsName, out textureLevels))
				{
					continue;
				}
				bool useHeadOffset = IsHeadLayer(textureLevels.renderLayer) || textureLevels.offsetToHead;
				PawnRenderNodeTagDef resolvedTag = ResolveNodeTagForMultiTex(textureLevels, apparel, useHeadOffset);
				float layerBase = baseLayer;
				if (resolvedTag == PawnRenderNodeTagDefOf.Body || resolvedTag == PawnRenderNodeTagDefOf.Head || resolvedTag == Building_NewOutfitStand.RootNodeTagDef)
				{
					layerBase = 0f;
				}
				float resolvedLayer = ResolveLayerForLevel(textureLevels, base.Rotation, layerBase);
				Building_NewOutfitStand.CachedMultiTexRenderInfo item = new Building_NewOutfitStand.CachedMultiTexRenderInfo(textureLevels, multiTexBatch, apparel, resolvedLayer, scale, positionOffset, useHeadOffset);
				if (useHeadOffset)
				{
					this.cachedMultiTexApparelHeadgear.Add(item);
				}
				else
				{
					this.cachedMultiTexApparelNonHeadgear.Add(item);
				}
			}
			return true;
		}

		private static bool IsHeadLayer(TextureRenderLayer layer)
		{
			switch (layer)
			{
			case TextureRenderLayer.BottomHair:
			case TextureRenderLayer.Head:
			case TextureRenderLayer.FaceMask:
			case TextureRenderLayer.Hair:
			case TextureRenderLayer.HeadMask:
			case TextureRenderLayer.Hat:
				return true;
			default:
				return false;
			}
		}

		private static float ResolveLayerForLevel(TextureLevels level, Rot4 rotation, float apparelBaseLayer)
		{
			float localLayer = level.baseLayer;
			if (level.drawData != null)
			{
				localLayer = level.drawData.LayerForRot(rotation, localLayer);
			}
			return apparelBaseLayer + localLayer;
		}

		private float GetNodeBaseLayer(RenderTreeLayout layout, PawnRenderNodeTagDef tag)
		{
			if (layout == null || tag == null)
			{
				return 0f;
			}
			PawnRenderNodeTagDef resolvedTag = ResolveNodeTag(layout, tag);
			RenderTreeNodeLayout nodeLayout;
			if (resolvedTag == null || !layout.nodesByTag.TryGetValue(resolvedTag, out nodeLayout) || nodeLayout.props == null)
			{
				return 0f;
			}
			return nodeLayout.props.baseLayer;
		}

		private static float ResolveVanillaApparelLayer(Apparel apparel, Rot4 rotation, float baseLayer)
		{
			DrawData drawData = apparel.def.apparel.drawData;
			if (drawData != null)
			{
				return drawData.LayerForRot(rotation, baseLayer);
			}
			if (!apparel.def.apparel.shellRenderedBehindHead)
			{
				ApparelLayerDef lastLayer = apparel.def.apparel.LastLayer;
				if (lastLayer == ApparelLayerDefOf.Shell && rotation == Rot4.North)
				{
					return 88f;
				}
				if (apparel.RenderAsPack())
				{
					if (rotation == Rot4.North)
					{
						return 93f;
					}
					if (rotation == Rot4.South)
					{
						return -3f;
					}
				}
			}
			return baseLayer;
		}

		private static bool RenderSwitchAllows(MultiTexBatch batch, Rot4 rotation)
		{
			if (batch == null)
			{
				return false;
			}
			if (batch.renderSwitch.x == 0f && rotation == Rot4.South)
			{
				return false;
			}
			if (batch.renderSwitch.y == 0f && rotation.IsHorizontal)
			{
				return false;
			}
			if (batch.renderSwitch.z == 0f && rotation == Rot4.North)
			{
				return false;
			}
			return true;
		}

		private Vector3 ResolveLevelOffset(TextureLevels level, Rot4 rotation, Vector3 local)
		{
			Vector3 vector = level.DrawOffsetForRot(rotation);
			if (level.useStaticYOffset)
			{
				local.y = local.y + vector.y * 0.01f;
			}
			vector = ApplyModelScaleToOffset(vector);
			if (level.usePublicYOffset)
			{
				vector.y *= 0.01f;
			}
			else
			{
				vector.y *= 0.0001f;
			}
			return local + vector;
		}

		private static Rot4 GetMatFacing(TextureLevels level, Rot4 rotation)
		{
			if (level.switchEastWest && rotation.IsHorizontal)
			{
				return new Rot4(4 - rotation.AsInt);
			}
			return rotation;
		}

		private static Vector2 GetMeshSize(TextureLevels level)
		{
			return (level.meshSize == Vector2.zero) ? new Vector2(1.5f, 1.5f) : level.meshSize;
		}

		private Vector3 GetStandOffset()
		{
			OutfitStandRenderPlanExtension modExtension = this.def.GetModExtension<OutfitStandRenderPlanExtension>();
			if (modExtension == null)
			{
				return Vector3.zero;
			}
			string raceName = GetEffectiveRaceDef() != null ? GetEffectiveRaceDef().defName : "";
			if (!raceName.NullOrEmpty() && !modExtension.raceOffsets.NullOrEmpty())
			{
				for (int i = 0; i < modExtension.raceOffsets.Count; i++)
				{
					OutfitStandRaceOffset offset = modExtension.raceOffsets[i];
					if (offset != null && offset.raceDefName == raceName)
					{
						return offset.offset;
					}
				}
			}
			return modExtension.standOffset;
		}

		private float GetModelBaseAltitudeOffset()
		{
			ThingDef raceDef = GetEffectiveRaceDef();
			float pawnAltitude = (raceDef != null) ? raceDef.Altitude : AltitudeLayer.Pawn.AltitudeFor();
			return pawnAltitude - this.def.Altitude;
		}

		private string RenderPlanDefName
		{
			get
			{
				OutfitStandRenderPlanExtension modExtension = this.def.GetModExtension<OutfitStandRenderPlanExtension>();
				return (modExtension != null) ? modExtension.renderPlanDef : null;
			}
		}

		private string EffectiveRenderPlanDefName
		{
			get
			{
				if (!selectedRaceDefName.NullOrEmpty() && ThisModData.RacePlansDatabase.ContainsKey(selectedRaceDefName))
				{
					return ThisModData.RacePlansDatabase[selectedRaceDefName].defName;
				}
				if (!RenderPlanDefName.NullOrEmpty() && ThisModData.RacePlansDatabase.ContainsKey(RenderPlanDefName))
				{
					return ThisModData.RacePlansDatabase[RenderPlanDefName].defName;
				}
				return RenderPlanDefName;
			}
		}

		private ThingDef GetSelectedRaceDef()
		{
			if (selectedRaceDefName.NullOrEmpty())
			{
				return null;
			}
			return DefDatabase<ThingDef>.GetNamedSilentFail(selectedRaceDefName);
		}

		private ThingDef GetEffectiveRaceDef()
		{
			ThingDef raceDef = GetSelectedRaceDef();
			if (raceDef != null)
			{
				return raceDef;
			}
			if (!RenderPlanDefName.NullOrEmpty())
			{
				ThingDef fallback = DefDatabase<ThingDef>.GetNamedSilentFail(RenderPlanDefName);
				if (fallback != null && fallback.race != null && fallback.race.Humanlike)
				{
					return fallback;
				}
			}
			return ThingDefOf.Human;
		}

		private Gender GetEffectiveGenderForRender()
		{
			HeadTypeDef headType = GetSelectedHeadTypeDef();
			if (headType != null && headType.gender != Gender.None)
			{
				return headType.gender;
			}
			return selectedGender;
		}

		private Vector2 GetAlienBodyScale()
		{
			ThingDef_AlienRace alienRace = GetEffectiveRaceDef() as ThingDef_AlienRace;
			if (alienRace != null && alienRace.alienRace != null)
			{
				AlienPartGenerator apg = alienRace.alienRace.generalSettings.alienPartGenerator;
				if (apg != null && apg.customDrawSize != Vector2.zero)
				{
					return apg.customDrawSize;
				}
			}
			ThingDef raceDef = GetEffectiveRaceDef();
			if (raceDef != null && raceDef.race != null && !raceDef.race.lifeStageAges.NullOrEmpty())
			{
				LifeStageAge lifeStage = raceDef.race.lifeStageAges.FirstOrDefault((LifeStageAge lsa) => lsa.def.developmentalStage.Juvenile() == this.IsJuvenileBodyType);
				LifeStageAgeAlien ageAlien = lifeStage as LifeStageAgeAlien;
				if (ageAlien != null && ageAlien.customDrawSize != Vector2.zero)
				{
					return ageAlien.customDrawSize;
				}
			}
			return Vector2.one;
		}

		private Vector2 GetAlienHeadScale()
		{
			ThingDef_AlienRace alienRace = GetEffectiveRaceDef() as ThingDef_AlienRace;
			if (alienRace != null && alienRace.alienRace != null)
			{
				AlienPartGenerator apg = alienRace.alienRace.generalSettings.alienPartGenerator;
				if (apg != null)
				{
					if (apg.customHeadDrawSize != Vector2.zero)
					{
						return apg.customHeadDrawSize;
					}
					if (apg.customDrawSize != Vector2.zero)
					{
						return apg.customDrawSize;
					}
				}
			}
			ThingDef raceDef = GetEffectiveRaceDef();
			if (raceDef != null && raceDef.race != null && !raceDef.race.lifeStageAges.NullOrEmpty())
			{
				LifeStageAge lifeStage = raceDef.race.lifeStageAges.FirstOrDefault((LifeStageAge lsa) => lsa.def.developmentalStage.Juvenile() == this.IsJuvenileBodyType);
				LifeStageAgeAlien ageAlien = lifeStage as LifeStageAgeAlien;
				if (ageAlien != null)
				{
					if (ageAlien.customHeadDrawSize != Vector2.zero)
					{
						return ageAlien.customHeadDrawSize;
					}
					if (ageAlien.customDrawSize != Vector2.zero)
					{
						return ageAlien.customDrawSize;
					}
				}
			}
			return Vector2.one;
		}

		private DummyExtendedGraphicsPawnWrapper BuildRenderDummy()
		{
			ThingDef raceDef = GetEffectiveRaceDef();
			if (raceDef == null || raceDef.race == null)
			{
				return null;
			}
			BodyTypeDef bodyType = GetSelectedBodyTypeDef();
			HeadTypeDef headType = GetSelectedHeadTypeDef();
			DummyExtendedGraphicsPawnWrapper dummy = new DummyExtendedGraphicsPawnWrapper
			{
				race = raceDef,
				bodyType = bodyType,
				headType = headType,
				gender = GetEffectiveGenderForRender(),
				body = raceDef.race.body,
				currentLifeStage = GetLifeStageDefForRace(raceDef, this.IsJuvenileBodyType)
			};
			return dummy;
		}

		private void UpdateAlienRaceGraphicOverrides()
		{
			ThingDef_AlienRace alienRace = GetEffectiveRaceDef() as ThingDef_AlienRace;
			if (alienRace == null || alienRace.alienRace == null || alienRace.alienRace.graphicPaths == null)
			{
				return;
			}
			AlienPartGenerator apg = alienRace.alienRace.generalSettings.alienPartGenerator;
			if (apg == null)
			{
				return;
			}
			int savedIndex = this.HashOffset();
			int shared = 0;
			string bodyPath = alienRace.alienRace.graphicPaths.body.GetPath(null, ref shared, new int?(savedIndex), null);
			Vector2 bodyDrawSize = this.IsJuvenileBodyType ? Building_NewOutfitStand.bodyChildDrawSize : Building_NewOutfitStand.bodyDrawSize;
			if (!bodyPath.NullOrEmpty())
			{
				this.standBodyGraphicOverride = CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), bodyPath, ShaderDatabase.Cutout, bodyDrawSize, Color.white, Color.white, null, 0, null, string.Empty));
			}
			string headPath = alienRace.alienRace.graphicPaths.head.GetPath(null, ref shared, new int?(savedIndex), null);
			Vector2 headDrawSize = Building_NewOutfitStand.headDrawSize;
			if (!headPath.NullOrEmpty())
			{
				this.standHeadGraphicOverride = CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), headPath, ShaderDatabase.Cutout, headDrawSize, Color.white, Color.white, null, 0, null, string.Empty));
			}
		}

		private static LifeStageDef GetLifeStageDefForRace(ThingDef raceDef, bool juvenile)
		{
			if (raceDef == null || raceDef.race == null || raceDef.race.lifeStageAges == null)
			{
				return null;
			}
			LifeStageAge lifeStage = raceDef.race.lifeStageAges.FirstOrDefault((LifeStageAge lsa) => lsa.def.developmentalStage.Juvenile() == juvenile);
			return (lifeStage != null) ? lifeStage.def : null;
		}

		private bool IsJuvenileBodyType
		{
			get
			{
				return this.BodyTypeDefForRendering == BodyTypeDefOf.Baby || this.BodyTypeDefForRendering == BodyTypeDefOf.Child;
			}
		}

		private Vector2 GetAlienHeadOffsetBase()
		{
			ThingDef_AlienRace alienRace = GetEffectiveRaceDef() as ThingDef_AlienRace;
			if (alienRace == null || alienRace.race == null)
			{
				return Vector2.zero;
			}
			Gender gender = GetEffectiveGenderForRender();
			AlienPartGenerator apg = alienRace.alienRace != null ? alienRace.alienRace.generalSettings.alienPartGenerator : null;
			if (apg != null)
			{
				if (gender == Gender.Female)
				{
					return apg.headFemaleOffset.Equals(Vector2.negativeInfinity) ? apg.headOffset : apg.headFemaleOffset;
				}
				return apg.headOffset;
			}
			LifeStageAge lifeStage = alienRace.race.lifeStageAges.NullOrEmpty() ? null : alienRace.race.lifeStageAges.FirstOrDefault((LifeStageAge lsa) => lsa.def.developmentalStage.Juvenile() == this.IsJuvenileBodyType);
			LifeStageAgeAlien ageAlien = lifeStage as LifeStageAgeAlien;
			if (ageAlien != null)
			{
				return (gender == Gender.Female) ? ageAlien.headFemaleOffset : ageAlien.headOffset;
			}
			return Vector2.zero;
		}

		private Vector3 GetAlienHeadOffsetDirectional(Rot4 rotation)
		{
			ThingDef_AlienRace alienRace = GetEffectiveRaceDef() as ThingDef_AlienRace;
			if (alienRace == null || alienRace.race == null)
			{
				return Vector3.zero;
			}
			Gender gender = GetEffectiveGenderForRender();
			AlienPartGenerator apg = alienRace.alienRace != null ? alienRace.alienRace.generalSettings.alienPartGenerator : null;
			AlienPartGenerator.DirectionalOffset directionalOffset = null;
			if (apg != null)
			{
				directionalOffset = (gender == Gender.Female) ? (apg.headFemaleOffsetDirectional ?? apg.headOffsetDirectional) : apg.headOffsetDirectional;
			}
			if (directionalOffset == null)
			{
				LifeStageAge lifeStage = alienRace.race.lifeStageAges.NullOrEmpty() ? null : alienRace.race.lifeStageAges.FirstOrDefault((LifeStageAge lsa) => lsa.def.developmentalStage.Juvenile() == this.IsJuvenileBodyType);
				LifeStageAgeAlien stageAgeAlien = lifeStage as LifeStageAgeAlien;
				directionalOffset = (gender == Gender.Female) ? ((stageAgeAlien != null) ? stageAgeAlien.headFemaleOffsetDirectional : null) : ((stageAgeAlien != null) ? stageAgeAlien.headOffsetDirectional : null);
			}
			if (directionalOffset == null)
			{
				return Vector3.zero;
			}
			AlienPartGenerator.RotationOffset offset = directionalOffset.GetOffset(rotation);
			return (offset != null) ? offset.GetOffset(false, this.BodyTypeDefForRendering, GetSelectedHeadTypeDef()) : Vector3.zero;
		}

		private BodyTypeDef GetSelectedBodyTypeDef()
		{
			List<BodyTypeDef> options = GetBodyTypeOptions();
			if (!selectedBodyTypeDefName.NullOrEmpty())
			{
				BodyTypeDef byName = DefDatabase<BodyTypeDef>.GetNamedSilentFail(selectedBodyTypeDefName);
				if (byName != null && (options.NullOrEmpty() || options.Contains(byName)))
				{
					return byName;
				}
			}
			ThingDef_AlienRace alienRace = GetEffectiveRaceDef() as ThingDef_AlienRace;
			if (alienRace != null && alienRace.alienRace != null)
			{
				AlienPartGenerator apg = alienRace.alienRace.generalSettings.alienPartGenerator;
				if (apg != null)
				{
					BodyTypeDef defaultBody = (GetEffectiveGenderForRender() == Gender.Female) ? apg.defaultFemaleBodyType : apg.defaultMaleBodyType;
					if (defaultBody != null)
					{
						return defaultBody;
					}
					if (!apg.bodyTypes.NullOrEmpty())
					{
						BodyTypeDef firstBody = apg.bodyTypes.FirstOrDefault((BodyTypeDef t) => t != null);
						if (firstBody != null)
						{
							return firstBody;
						}
					}
				}
			}
			if (!options.NullOrEmpty())
			{
				if (GetEffectiveGenderForRender() == Gender.Female && BodyTypeDefOf.Female != null && options.Contains(BodyTypeDefOf.Female))
				{
					return BodyTypeDefOf.Female;
				}
				return options.FirstOrDefault() ?? BodyTypeDefOf.Male;
			}
			if (GetEffectiveGenderForRender() == Gender.Female && BodyTypeDefOf.Female != null)
			{
				return BodyTypeDefOf.Female;
			}
			return BodyTypeDefOf.Male;
		}

		private HeadTypeDef GetSelectedHeadTypeDef()
		{
			List<HeadTypeDef> options = GetHeadTypeOptions();
			if (!selectedHeadTypeDefName.NullOrEmpty())
			{
				HeadTypeDef byName = DefDatabase<HeadTypeDef>.GetNamedSilentFail(selectedHeadTypeDefName);
				if (byName != null && (options.NullOrEmpty() || options.Contains(byName)))
				{
					return byName;
				}
			}
			if (!options.NullOrEmpty())
			{
				HeadTypeDef head = options.FirstOrDefault((HeadTypeDef x) => x != HeadTypeDefOf.Skull && x != HeadTypeDefOf.Stump);
				return head ?? options.FirstOrDefault();
			}
			List<HeadTypeDef> heads = DefDatabase<HeadTypeDef>.AllDefsListForReading;
			HeadTypeDef fallback = heads.FirstOrDefault((HeadTypeDef x) => x != HeadTypeDefOf.Skull && x != HeadTypeDefOf.Stump);
			return fallback ?? heads.FirstOrDefault() ?? HeadTypeDefOf.Skull;
		}

		private IEnumerable<ThingDef> GetRaceOptions()
		{
			if (!ThisModData.RacePlansDatabase.NullOrEmpty())
			{
				foreach (string raceName in ThisModData.RacePlansDatabase.Keys)
				{
					ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(raceName);
					if (def != null)
					{
						yield return def;
					}
				}
				yield break;
			}
			foreach (ThingDef def2 in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (def2.race != null && def2.race.Humanlike)
				{
					yield return def2;
				}
			}
		}

		private List<BodyTypeDef> GetBodyTypeOptions()
		{
			List<BodyTypeDef> raceList = GetBodyTypeOptionsFromRace(GetEffectiveRaceDef());
			if (!raceList.NullOrEmpty())
			{
				return raceList;
			}
			List<BodyTypeDef> list = GetBodyTypeOptionsFromPlan(EffectiveRenderPlanDefName);
			if (!list.NullOrEmpty())
			{
				return list;
			}
			return new List<BodyTypeDef>(DefDatabase<BodyTypeDef>.AllDefsListForReading);
		}

		private List<HeadTypeDef> GetHeadTypeOptions()
		{
			List<HeadTypeDef> raceList = GetHeadTypeOptionsFromRace(GetEffectiveRaceDef());
			if (!raceList.NullOrEmpty())
			{
				return raceList;
			}
			List<HeadTypeDef> list = GetHeadTypeOptionsFromPlan(EffectiveRenderPlanDefName);
			if (!list.NullOrEmpty())
			{
				return list;
			}
			return new List<HeadTypeDef>(DefDatabase<HeadTypeDef>.AllDefsListForReading);
		}

		private static List<BodyTypeDef> GetBodyTypeOptionsFromRace(ThingDef raceDef)
		{
			ThingDef_AlienRace alienRace = raceDef as ThingDef_AlienRace;
			if (alienRace == null || alienRace.alienRace == null)
			{
				return null;
			}
			AlienPartGenerator apg = alienRace.alienRace.generalSettings.alienPartGenerator;
			if (apg == null || apg.bodyTypes.NullOrEmpty())
			{
				return null;
			}
			return apg.bodyTypes.Where((BodyTypeDef t) => t != null).ToList();
		}

		private static List<HeadTypeDef> GetHeadTypeOptionsFromRace(ThingDef raceDef)
		{
			ThingDef_AlienRace alienRace = raceDef as ThingDef_AlienRace;
			if (alienRace == null || alienRace.alienRace == null)
			{
				return null;
			}
			AlienPartGenerator apg = alienRace.alienRace.generalSettings.alienPartGenerator;
			if (apg == null || apg.HeadTypes.NullOrEmpty())
			{
				return null;
			}
			return apg.HeadTypes.Where((HeadTypeDef t) => t != null).ToList();
		}

		private static List<BodyTypeDef> GetBodyTypeOptionsFromPlan(string planName)
		{
			Dictionary<string, MultiTexDef> plan;
			if (planName.NullOrEmpty() || !ThisModData.DefAndKeyDatabase.TryGetValue(planName, out plan))
			{
				return null;
			}
			HashSet<BodyTypeDef> result = new HashSet<BodyTypeDef>();
			foreach (MultiTexDef def in plan.Values)
			{
				if (def != null && def.originalDefClass == typeof(BodyTypeDef))
				{
					BodyTypeDef body = DefDatabase<BodyTypeDef>.GetNamedSilentFail(def.originalDef);
					if (body != null)
					{
						result.Add(body);
					}
				}
			}
			return result.ToList();
		}

		private static List<HeadTypeDef> GetHeadTypeOptionsFromPlan(string planName)
		{
			Dictionary<string, MultiTexDef> plan;
			if (planName.NullOrEmpty() || !ThisModData.DefAndKeyDatabase.TryGetValue(planName, out plan))
			{
				return null;
			}
			HashSet<HeadTypeDef> result = new HashSet<HeadTypeDef>();
			foreach (MultiTexDef def in plan.Values)
			{
				if (def != null && def.originalDefClass == typeof(HeadTypeDef))
				{
					HeadTypeDef head = DefDatabase<HeadTypeDef>.GetNamedSilentFail(def.originalDef);
					if (head != null)
					{
						result.Add(head);
					}
				}
			}
			return result.ToList();
		}

		private Vector3 HeadOffsetAt(Rot4 rotation)
		{
			Vector2 headOffset = this.BodyTypeDefForRendering.headOffset + GetAlienHeadOffsetBase();
			Vector3 vector;
			switch (rotation.AsInt)
			{
			case 0:
				vector = new Vector3(0f, 0f, headOffset.y);
				break;
			case 1:
				vector = new Vector3(headOffset.x, 0f, headOffset.y);
				break;
			case 2:
				vector = new Vector3(0f, 0f, headOffset.y);
				break;
			case 3:
				vector = new Vector3(-headOffset.x, 0f, headOffset.y);
				break;
			default:
				vector = Vector3.zero;
				break;
			}
			return vector + GetAlienHeadOffsetDirectional(rotation);
		}

		private RenderTreeLayout GetRenderTreeLayout()
		{
			ThingDef raceDef = GetEffectiveRaceDef();
			string raceName = (raceDef != null) ? raceDef.defName : "";
			if (this.cachedRenderTreeLayout == null || this.cachedRenderTreeRaceDefName != raceName)
			{
				this.cachedRenderTreeLayout = BuildRenderTreeLayout(raceDef);
				this.cachedRenderTreeRaceDefName = raceName;
			}
			return this.cachedRenderTreeLayout;
		}

		private RenderTreeLayout BuildRenderTreeLayout(ThingDef raceDef)
		{
			PawnRenderTreeDef renderTree = (raceDef != null && raceDef.race != null) ? raceDef.race.renderTree : null;
			if (renderTree == null || renderTree.root == null)
			{
				ThingDef fallback = ThingDefOf.Human;
				renderTree = (fallback != null && fallback.race != null) ? fallback.race.renderTree : null;
			}
			PawnRenderNodeProperties root = (renderTree != null) ? renderTree.root : null;
			RenderTreeLayout layout = new RenderTreeLayout();
			if (root == null)
			{
				layout.EnsureFallbackNodes(Building_NewOutfitStand.RootNodeTagDef);
				return layout;
			}
			List<PawnRenderNodeProperties> ancestry = new List<PawnRenderNodeProperties>();
			int orderIndex = 0;
			TraverseRenderTree(root, ancestry, layout, ref orderIndex);
			layout.EnsureFallbackNodes(Building_NewOutfitStand.RootNodeTagDef);
			return layout;
		}

		private static void TraverseRenderTree(PawnRenderNodeProperties props, List<PawnRenderNodeProperties> ancestry, RenderTreeLayout layout, ref int orderIndex)
		{
			if (props == null)
			{
				return;
			}
			ancestry.Add(props);
			int currentOrder = orderIndex++;
			if (props.tagDef != null && !layout.nodesByTag.ContainsKey(props.tagDef))
			{
				layout.nodesByTag.Add(props.tagDef, new RenderTreeNodeLayout(props, ancestry.ToArray(), currentOrder));
			}
			if (!props.children.NullOrEmpty<PawnRenderNodeProperties>())
			{
				for (int i = 0; i < props.children.Count; i++)
				{
					TraverseRenderTree(props.children[i], ancestry, layout, ref orderIndex);
				}
			}
			ancestry.RemoveAt(ancestry.Count - 1);
		}

		private NodeTransform GetNodeTransform(RenderTreeLayout layout, PawnRenderNodeTagDef tag, Rot4 rot)
		{
			if (layout == null || tag == null)
			{
				return NodeTransform.Default;
			}
			PawnRenderNodeTagDef resolvedTag = ResolveNodeTag(layout, tag);
			RenderTreeNodeLayout nodeLayout;
			if (resolvedTag == null || !layout.nodesByTag.TryGetValue(resolvedTag, out nodeLayout))
			{
				return NodeTransform.Default;
			}
			NodeTransform transform = ComputeNodeTransform(nodeLayout, rot);
			transform.scale = ApplyAlienScale(resolvedTag, transform.scale);
			transform = ApplyModelScale(transform);
			transform = ApplyModelZOffset(transform);
			transform.order = nodeLayout.order;
			return transform;
		}

		private NodeTransform ComputeNodeTransform(RenderTreeNodeLayout nodeLayout, Rot4 rot)
		{
			Vector3 offset = Vector3.zero;
			Vector3 scale = Vector3.one;
			PawnRenderNodeProperties[] ancestors = nodeLayout.ancestors;
			for (int i = 0; i < ancestors.Length; i++)
			{
				PawnRenderNodeProperties props = ancestors[i];
				if (props.drawData != null)
				{
					Vector3 localOffset = props.drawData.OffsetForRot(rot);
					if (props.drawData.scaleOffsetByBodySize && this.BodyTypeDefForRendering != null)
					{
						Vector2 bodyScale = this.BodyTypeDefForRendering.bodyGraphicScale;
						float factor = (bodyScale.x + bodyScale.y) * 0.5f;
						localOffset *= factor;
					}
					offset += localOffset;
				}
				Vector2 drawSize = props.drawSize;
				scale.x *= drawSize.x;
				scale.z *= drawSize.y;
			}
			float layer = 0f;
			PawnRenderNodeProperties nodeProps = nodeLayout.props;
			if (nodeProps != null)
			{
				layer = (nodeProps.drawData != null) ? nodeProps.drawData.LayerForRot(rot, nodeProps.baseLayer) : nodeProps.baseLayer;
			}
			return new NodeTransform(offset, scale, layer, nodeLayout.order);
		}

		private static PawnRenderNodeTagDef ResolveNodeTag(RenderTreeLayout layout, PawnRenderNodeTagDef tag)
		{
			if (tag != null && layout.nodesByTag.ContainsKey(tag))
			{
				return tag;
			}
			if (tag == PawnRenderNodeTagDefOf.ApparelBody && layout.nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.Body))
			{
				return PawnRenderNodeTagDefOf.Body;
			}
			if (tag == PawnRenderNodeTagDefOf.ApparelHead && layout.nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.Head))
			{
				return PawnRenderNodeTagDefOf.Head;
			}
			PawnRenderNodeTagDef rootTag = Building_NewOutfitStand.RootNodeTagDef;
			if (rootTag != null && layout.nodesByTag.ContainsKey(rootTag))
			{
				return rootTag;
			}
			if (layout.nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.Body))
			{
				return PawnRenderNodeTagDefOf.Body;
			}
			return tag;
		}

		private static PawnRenderNodeTagDef ResolveNodeTagForMultiTex(TextureLevels level, Apparel apparel, bool isHeadgear)
		{
			if (level != null && level.renderParentNodeTagDef != null)
			{
				return level.renderParentNodeTagDef;
			}
			if (level != null)
			{
				return GetDefaultNodeTagForLayer(level.renderLayer);
			}
			if (apparel != null && apparel.def != null && apparel.def.apparel != null && apparel.def.apparel.parentTagDef != null)
			{
				return apparel.def.apparel.parentTagDef;
			}
			return isHeadgear ? PawnRenderNodeTagDefOf.ApparelHead : PawnRenderNodeTagDefOf.ApparelBody;
		}

		private static PawnRenderNodeTagDef GetDefaultNodeTagForLayer(TextureRenderLayer layer)
		{
			switch (layer)
			{
			case TextureRenderLayer.BottomOverlay:
				return Building_NewOutfitStand.RootNodeTagDef;
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
				return Building_NewOutfitStand.RootNodeTagDef;
			default:
				return Building_NewOutfitStand.RootNodeTagDef;
			}
		}

		private Vector3 ApplyAlienScale(PawnRenderNodeTagDef tag, Vector3 scale)
		{
			if (tag == PawnRenderNodeTagDefOf.Body || tag == PawnRenderNodeTagDefOf.ApparelBody)
			{
				Vector2 bodyScale = GetAlienBodyScale();
				scale.x *= bodyScale.x;
				scale.z *= bodyScale.y;
			}
			if (tag == PawnRenderNodeTagDefOf.Head || tag == PawnRenderNodeTagDefOf.ApparelHead)
			{
				Vector2 headScale = GetAlienHeadScale();
				scale.x *= headScale.x;
				scale.z *= headScale.y;
			}
			return scale;
		}

		private NodeTransform ApplyModelScale(NodeTransform transform)
		{
			if (Mathf.Abs(this.modelScale - 1f) < 0.0001f)
			{
				return transform;
			}
			transform.scale.x *= this.modelScale;
			transform.scale.z *= this.modelScale;
			transform.offset.x *= this.modelScale;
			transform.offset.z *= this.modelScale;
			return transform;
		}

		private Vector3 ApplyModelScaleToOffset(Vector3 offset)
		{
			if (Mathf.Abs(this.modelScale - 1f) < 0.0001f)
			{
				return offset;
			}
			offset.x *= this.modelScale;
			offset.z *= this.modelScale;
			return offset;
		}

		private NodeTransform ApplyModelZOffset(NodeTransform transform)
		{
			if (Mathf.Abs(this.modelZOffset) < 0.0001f)
			{
				return transform;
			}
			transform.offset.z += this.modelZOffset;
			return transform;
		}

		private void AddVanillaDrawItem(Building_NewOutfitStand.CachedGraphicRenderInfo info, PawnRenderNodeTagDef nodeTag, RenderTreeLayout layout, Rot4 rot)
		{
			NodeTransform transform = GetNodeTransform(layout, nodeTag, rot);
			DrawItem item = new DrawItem
			{
				type = Building_NewOutfitStand.DrawItemType.Vanilla,
				vanilla = info,
				nodeOffset = transform.offset,
				nodeScale = transform.scale,
				layer = transform.layer + info.layer,
				order = transform.order
			};
			this.drawItems.Add(item);
		}

		private void AddMultiTexDrawItem(Building_NewOutfitStand.CachedMultiTexRenderInfo info, PawnRenderNodeTagDef nodeTag, RenderTreeLayout layout, Rot4 rot)
		{
			NodeTransform transform = GetNodeTransform(layout, nodeTag, rot);
			DrawItem item = new DrawItem
			{
				type = Building_NewOutfitStand.DrawItemType.MultiTex,
				multi = info,
				nodeOffset = transform.offset,
				nodeScale = transform.scale,
				layer = info.layer,
				order = transform.order
			};
			this.drawItems.Add(item);
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (!Building_NewOutfitStand.initializedTextures)
			{
				Building_NewOutfitStand.InitGraphics();
			}
			bool flag = this.BodyTypeDefForRendering == BodyTypeDefOf.Child;
			Graphic coloredVersion = Building_NewOutfitStand.baseGraphic.GetColoredVersion(Building_NewOutfitStand.baseGraphic.Shader, this.DrawColor, this.DrawColorTwo);
			Vector3 vector = (flag ? new Vector3(drawLoc.x, drawLoc.y, drawLoc.z + 0.2f) : drawLoc);
			Rot4 rot = (flip ? base.Rotation.Opposite : base.Rotation);
			coloredVersion.Draw(vector, rot, this, 0f);
			Vector3 standDrawLoc = drawLoc + GetStandOffset();
			RenderTreeLayout renderTreeLayout = GetRenderTreeLayout();
			this.drawItems.Clear();
			if (!this.hideOriginalBody)
			{
				Graphic_Multi bodyGraphic = this.standBodyGraphicOverride ?? (flag ? Building_NewOutfitStand.bodyGraphicChild : Building_NewOutfitStand.bodyGraphic);
				Graphic bodyColored = bodyGraphic.GetColoredVersion(bodyGraphic.Shader, this.DrawColor, this.DrawColorTwo);
				AddVanillaDrawItem(new Building_NewOutfitStand.CachedGraphicRenderInfo(bodyColored, 0f, Vector3.one, Vector3.zero, false), PawnRenderNodeTagDefOf.Body, renderTreeLayout, rot);
			}
			if (!this.cachedMultiTexStandBody.NullOrEmpty())
			{
				for (int i = 0; i < this.cachedMultiTexStandBody.Count; i++)
				{
					Building_NewOutfitStand.CachedMultiTexRenderInfo cachedMulti = this.cachedMultiTexStandBody[i];
					PawnRenderNodeTagDef nodeTagDef = ResolveNodeTagForMultiTex(cachedMulti.textureLevels, cachedMulti.apparel, false);
					AddMultiTexDrawItem(cachedMulti, nodeTagDef, renderTreeLayout, rot);
				}
			}
			if (!this.cachedApparelGraphicsNonHeadgear.NullOrEmpty())
			{
				for (int j = 0; j < this.cachedApparelGraphicsNonHeadgear.Count; j++)
				{
					AddVanillaDrawItem(this.cachedApparelGraphicsNonHeadgear[j], PawnRenderNodeTagDefOf.ApparelBody, renderTreeLayout, rot);
				}
			}
			if (!this.cachedMultiTexApparelNonHeadgear.NullOrEmpty())
			{
				for (int k = 0; k < this.cachedMultiTexApparelNonHeadgear.Count; k++)
				{
					Building_NewOutfitStand.CachedMultiTexRenderInfo cachedMulti2 = this.cachedMultiTexApparelNonHeadgear[k];
					PawnRenderNodeTagDef nodeTagDef2 = ResolveNodeTagForMultiTex(cachedMulti2.textureLevels, cachedMulti2.apparel, false);
					AddMultiTexDrawItem(cachedMulti2, nodeTagDef2, renderTreeLayout, rot);
				}
			}
			if (!this.cachedApparelRenderInfoSkipHead)
			{
				if (!this.cachedApparelGraphicsHeadgear.NullOrEmpty())
				{
					for (int l = 0; l < this.cachedApparelGraphicsHeadgear.Count; l++)
					{
						AddVanillaDrawItem(this.cachedApparelGraphicsHeadgear[l], PawnRenderNodeTagDefOf.ApparelHead, renderTreeLayout, rot);
					}
				}
				if (!this.cachedMultiTexApparelHeadgear.NullOrEmpty())
				{
					for (int m = 0; m < this.cachedMultiTexApparelHeadgear.Count; m++)
					{
						Building_NewOutfitStand.CachedMultiTexRenderInfo cachedMulti3 = this.cachedMultiTexApparelHeadgear[m];
						PawnRenderNodeTagDef nodeTagDef3 = ResolveNodeTagForMultiTex(cachedMulti3.textureLevels, cachedMulti3.apparel, true);
						AddMultiTexDrawItem(cachedMulti3, nodeTagDef3, renderTreeLayout, rot);
					}
				}
				if (!this.cachedMultiTexStandHead.NullOrEmpty())
				{
					for (int n = 0; n < this.cachedMultiTexStandHead.Count; n++)
					{
						Building_NewOutfitStand.CachedMultiTexRenderInfo cachedMulti4 = this.cachedMultiTexStandHead[n];
						PawnRenderNodeTagDef nodeTagDef4 = ResolveNodeTagForMultiTex(cachedMulti4.textureLevels, cachedMulti4.apparel, true);
						AddMultiTexDrawItem(cachedMulti4, nodeTagDef4, renderTreeLayout, rot);
					}
				}
				if (!this.hideOriginalHead)
				{
					Graphic_Multi headGraphic = this.standHeadGraphicOverride ?? Building_NewOutfitStand.headGraphic;
					Graphic headColored = headGraphic.GetColoredVersion(headGraphic.Shader, this.DrawColor, this.DrawColorTwo);
					AddVanillaDrawItem(new Building_NewOutfitStand.CachedGraphicRenderInfo(headColored, 0f, Vector3.one, Vector3.zero, true), PawnRenderNodeTagDefOf.Head, renderTreeLayout, rot);
				}
			}
			if (this.drawItems.Count > 1)
			{
				this.drawItems.Sort((Building_NewOutfitStand.DrawItem a, Building_NewOutfitStand.DrawItem b) =>
				{
					int num = a.layer.CompareTo(b.layer);
					if (num != 0)
					{
						return num;
					}
					return a.order.CompareTo(b.order);
				});
			}
			if (this.drawItems.Count > 0)
			{
				Mesh mesh = MeshPool.GetMeshSetForSize(1.5f, 1.5f).MeshAt(rot);
				float baseAltitude = GetModelBaseAltitudeOffset();
				for (int num = 0; num < this.drawItems.Count; num++)
				{
					Building_NewOutfitStand.DrawItem drawItem = this.drawItems[num];
					if (drawItem.type == Building_NewOutfitStand.DrawItemType.Vanilla)
					{
						DrawVanillaEntry(drawItem.vanilla, standDrawLoc, rot, mesh, baseAltitude, drawItem.nodeOffset, drawItem.nodeScale, drawItem.layer);
					}
					else
					{
						DrawMultiTexEntry(drawItem.multi, standDrawLoc, rot, baseAltitude, drawItem.nodeOffset, drawItem.nodeScale, drawItem.layer);
					}
				}
			}
			if (this.holdingWeaponCached)
			{
				Vector3 vector7;
				if (rot == Rot4.North)
				{
					vector7 = standDrawLoc.WithYOffset(-0.05f);
				}
				else
				{
					vector7 = standDrawLoc.WithY(AltitudeLayer.ItemImportant.AltitudeFor() + PawnRenderUtility.AltitudeForLayer(90f));
				}
				PawnRenderUtility.DrawCarriedWeapon(this.HeldWeapon, vector7, flip ? base.Rotation.Opposite : base.Rotation, this.WeaponDrawDistanceFactor);
			}
		}

		private void DrawMergedApparel(List<Building_NewOutfitStand.CachedGraphicRenderInfo> vanilla, List<Building_NewOutfitStand.CachedMultiTexRenderInfo> multi, Vector3 drawLoc, Rot4 rot, Mesh vanillaMesh, float baseAltitude)
		{
			int vanillaIndex = 0;
			int multiIndex = 0;
			while (vanillaIndex < vanilla.Count || multiIndex < multi.Count)
			{
				float vanillaLayer = (vanillaIndex < vanilla.Count) ? vanilla[vanillaIndex].layer : float.MaxValue;
				float multiLayer = (multiIndex < multi.Count) ? multi[multiIndex].layer : float.MaxValue;
				if (multiLayer <= vanillaLayer)
				{
					DrawMultiTexEntry(multi[multiIndex], drawLoc, rot, baseAltitude, Vector3.zero, Vector3.one, multi[multiIndex].layer);
					multiIndex++;
				}
				else
				{
					DrawVanillaEntry(vanilla[vanillaIndex], drawLoc, rot, vanillaMesh, baseAltitude, Vector3.zero, Vector3.one, vanilla[vanillaIndex].layer);
					vanillaIndex++;
				}
			}
		}

		private void DrawVanillaEntry(Building_NewOutfitStand.CachedGraphicRenderInfo info, Vector3 drawLoc, Rot4 rot, Mesh mesh, float baseAltitude, Vector3 nodeOffset, Vector3 nodeScale, float layer)
		{
			Vector3 vector = drawLoc + nodeOffset;
			if (info.useHeadOffset)
			{
				vector += ApplyModelScaleToOffset(this.HeadOffsetAt(rot));
			}
			vector += ApplyModelScaleToOffset(info.positionOffset);
			vector.y += baseAltitude + PawnRenderUtility.AltitudeForLayer(layer);
			Graphic graphic = info.graphic;
			Material material = graphic.MatAt(rot, null);
			Vector3 vector2 = graphic.DrawOffset(rot);
			Vector3 scale = info.scale;
			scale.x *= nodeScale.x;
			scale.y *= nodeScale.y;
			scale.z *= nodeScale.z;
			Quaternion quaternion = graphic.QuatFromRot(rot);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector + vector2, quaternion, scale);
			Graphics.DrawMesh(mesh, matrix4x, material, 0);
		}

		private void DrawMultiTexEntry(Building_NewOutfitStand.CachedMultiTexRenderInfo info, Vector3 drawLoc, Rot4 rot, float baseAltitude, Vector3 nodeOffset, Vector3 nodeScale, float layer)
		{
			if (!RenderSwitchAllows(info.batch, rot))
			{
				return;
			}
			if (!TryApplyGenderSuffix(info.textureLevels))
			{
				return;
			}
			Vector3 vector = drawLoc + nodeOffset;
			if (info.useHeadOffset)
			{
				vector += ApplyModelScaleToOffset(this.HeadOffsetAt(rot));
			}
			vector += ApplyModelScaleToOffset(info.positionOffset);
			TextureLevels textureLevels = info.textureLevels;
			vector = ResolveLevelOffset(textureLevels, rot, vector);
			float altitude = baseAltitude + PawnRenderUtility.AltitudeForLayer(layer);
			vector.y += altitude;
			Vector2 meshSize = GetMeshSize(textureLevels);
			NareisLib_GraphicMeshSet meshSetForSize = NareisLib_MeshPool.GetMeshSetForSize(meshSize.x, meshSize.y);
			Mesh mesh = meshSetForSize.MeshAt(rot, textureLevels.flipped);
			Color color = (info.apparel != null) ? info.apparel.DrawColor : Color.white;
			Color colorTwo = (info.apparel != null) ? info.apparel.DrawColorTwo : Color.white;
			string bodyType = textureLevels.useBodyType ? this.BodyTypeDefForRendering.defName : "";
			string headType = textureLevels.useHeadType ? GetSelectedHeadTypeDef().defName : "";
			Graphic graphic = textureLevels.GetGraphic(info.batch.keyName, color, colorTwo, "", bodyType, headType);
			Rot4 matFacing = GetMatFacing(textureLevels, rot);
			Material material = graphic.MatAt(matFacing, null);
			Vector3 scale = info.scale;
			scale.x *= nodeScale.x;
			scale.y *= nodeScale.y;
			scale.z *= nodeScale.z;
			scale.x *= textureLevels.drawSize.x;
			scale.z *= textureLevels.drawSize.y;
			Quaternion quaternion = graphic.QuatFromRot(rot);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector, quaternion, scale);
			Graphics.DrawMesh(mesh, matrix4x, material, 0);
		}

		private void DrawMultiTexList(List<Building_NewOutfitStand.CachedMultiTexRenderInfo> list, Vector3 drawLoc, Rot4 rot, float baseAltitude)
		{
			if (list.NullOrEmpty())
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				DrawMultiTexEntry(list[i], drawLoc, rot, baseAltitude, Vector3.zero, Vector3.one, list[i].layer);
			}
		}

		private bool TryApplyGenderSuffix(TextureLevels level)
		{
			if (!level.hasGender)
			{
				level.genderSuffix = "";
				return true;
			}
			Gender effectiveGender = GetEffectiveGenderForRender();
			if (effectiveGender == Gender.Female)
			{
				if (!level.renderFemale)
				{
					return false;
				}
				level.genderSuffix = "_Female";
				return true;
			}
			if (!level.renderMale)
			{
				return false;
			}
			level.genderSuffix = "_Male";
			return true;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
			if (!selPawn.IsColonistPlayerControlled)
			{
				yield break;
			}
			if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
			{
				yield return new FloatMenuOption("CannotSwapOutfit".Translate().CapitalizeFirst() + ": " + "NoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
				yield break;
			}
			if (!this.innerContainer.Any)
			{
				yield return new FloatMenuOption("CannotSwapOutfit".Translate().CapitalizeFirst() + ": " + "OutfitStandEmpty".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
				yield break;
			}
			FloatMenuOption floatMenuOption = new FloatMenuOption("SwapOutfit".Translate().CapitalizeFirst(), delegate
			{
				this.SetAllowHauling(false);
				selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.UseOutfitStand, this), new JobTag?(JobTag.Misc), false);
			}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			yield return FloatMenuUtility.DecoratePrioritizedTask(floatMenuOption, selPawn, this, "ReservedBy", null);
			foreach (FloatMenuOption floatMenuOption2 in HaulSourceUtility.GetFloatMenuOptions(this, selPawn))
			{
				yield return floatMenuOption2;
			}
			IEnumerator<FloatMenuOption> enumerator = null;
			foreach (Thing thing in this.innerContainer.InnerListForReading)
			{
				Apparel ap = thing as Apparel;
				if (ap != null)
				{
					FloatMenuOption floatMenuOptionToWear = this.GetFloatMenuOptionToWear(selPawn, ap);
					yield return floatMenuOptionToWear;
					FloatMenuOption floatMenuOptionForForceWear = this.GetFloatMenuOptionForForceWear(selPawn, ap);
					yield return floatMenuOptionForForceWear;
					ap = null;
				}
			}
			List<Thing>.Enumerator enumerator2 = default(List<Thing>.Enumerator);
			if (this.holdingWeaponCached)
			{
				yield return this.GetFloatMenuOptionToEquipWeapon(selPawn, this.HeldWeapon);
			}
			yield break;
			yield break;
		}

		private FloatMenuOption GetFloatMenuOptionToWear(Pawn selPawn, Apparel apparel)
		{
			string text = "CannotWear";
			string text2 = "ForceWear";
			if (apparel.def.apparel.LastLayer.IsUtilityLayer)
			{
				text = "CannotEquipApparel";
				text2 = "ForceEquipApparel";
			}
			if (!selPawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
			{
				return new FloatMenuOption(text.Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (apparel.IsBurning())
			{
				return new FloatMenuOption(text.Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (selPawn.apparel.WouldReplaceLockedApparel(apparel))
			{
				return new FloatMenuOption(text.Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (selPawn.IsMutant && selPawn.mutant.Def.disableApparel)
			{
				return new FloatMenuOption(text.Translate(apparel.Label, apparel) + ": " + selPawn.mutant.Def.LabelCap, null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (!ApparelUtility.HasPartsToWear(selPawn, apparel.def))
			{
				return new FloatMenuOption(text.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			string text3;
			if (!EquipmentUtility.CanEquip(apparel, selPawn, out text3, true))
			{
				return new FloatMenuOption(text.Translate(apparel.Label, apparel) + ": " + text3, null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2.Translate(apparel.LabelShort, apparel), delegate
			{
				Action wearAction = delegate
				{
					selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Wear, apparel), new JobTag?(JobTag.Misc), false);
				};
				Apparel apparelReplacedByNewApparel = ApparelUtility.GetApparelReplacedByNewApparel(selPawn, apparel);
				if (apparelReplacedByNewApparel == null || !ModsConfig.BiotechActive || !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(selPawn, apparelReplacedByNewApparel, wearAction))
				{
					wearAction();
				}
			}, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), selPawn, this, "ReservedBy", null);
		}

		private FloatMenuOption GetFloatMenuOptionForForceWear(Pawn selPawn, Apparel apparel)
		{
			string cannotForceTargetText = "CannotForceTargetToWear";
			string text = "ForceTargetToWear";
			if (apparel.def.apparel.LastLayer.IsUtilityLayer)
			{
				cannotForceTargetText = "CannotForceTargetToEquipApparel";
				text = "ForceTargetToEquipApparel";
			}
			if (!selPawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
			{
				return new FloatMenuOption(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (apparel.IsBurning())
			{
				return new FloatMenuOption(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), null, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text.Translate(apparel.LabelShort, apparel), delegate
			{
				bool queueOrder = KeyBindingDefOf.QueueOrder.IsDownEvent;
				Find.Targeter.BeginTargeting(TargetingParameters.ForForceWear(selPawn), delegate(LocalTargetInfo target)
				{
					Pawn targetPawn;
					if (!target.TryGetPawn(out targetPawn))
					{
						if (ModsConfig.OdysseyActive)
						{
							Building_NewOutfitStand Building_NewOutfitStand = target.Thing as Building_NewOutfitStand;
							if (Building_NewOutfitStand != null)
							{
								if (!Building_NewOutfitStand.CanEverStoreThing(apparel))
								{
									Messages.Message("CannotStoreThingOnTarget".Translate(apparel.Named("THING"), Building_NewOutfitStand.Named("TARGET")), MessageTypeDefOf.RejectInput, false);
									return;
								}
								Pawn_JobTracker jobs = selPawn.jobs;
								Job job = JobMaker.MakeJob(JobDefOf.PutApparelOnOutfitStand, apparel, Building_NewOutfitStand);
								bool queueOrder3 = queueOrder;
								jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), queueOrder3);
							}
						}
						return;
					}
					if (targetPawn.apparel.WouldReplaceLockedApparel(apparel))
					{
						Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), targetPawn, MessageTypeDefOf.RejectInput, false);
						return;
					}
					if (targetPawn.IsMutant && targetPawn.mutant.Def.disableApparel)
					{
						Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + targetPawn.mutant.Def.LabelCap, targetPawn, MessageTypeDefOf.RejectInput, false);
						return;
					}
					if (!ApparelUtility.HasPartsToWear(targetPawn, apparel.def))
					{
						Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), targetPawn, MessageTypeDefOf.RejectInput, false);
						return;
					}
					string text2;
					if (!EquipmentUtility.CanEquip(apparel, targetPawn, out text2, true))
					{
						Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + text2, targetPawn, MessageTypeDefOf.RejectInput, false);
						return;
					}
					Action action = delegate
					{
						Pawn_JobTracker jobs2 = selPawn.jobs;
						Job job2 = JobMaker.MakeJob(JobDefOf.ForceTargetWear, targetPawn, apparel);
						bool queueOrder2 = queueOrder;
						jobs2.TryTakeOrderedJob(job2, new JobTag?(JobTag.Misc), queueOrder2);
					};
					Apparel apparelReplacedByNewApparel = ApparelUtility.GetApparelReplacedByNewApparel(targetPawn, apparel);
					if (apparelReplacedByNewApparel == null || !ModsConfig.BiotechActive || !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(targetPawn, apparelReplacedByNewApparel, action))
					{
						action();
					}
				}, null, null, null, true);
			}, apparel, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), selPawn, this, "ReservedBy", null);
		}

		private FloatMenuOption GetFloatMenuOptionToEquipWeapon(Pawn selPawn, Thing weapon)
		{
			if (!weapon.HasComp<CompEquippable>())
			{
				return null;
			}
			string labelShort = weapon.LabelShort;
			if (weapon.def.IsWeapon && selPawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfViolenceLower".Translate(selPawn.LabelShort, selPawn), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (weapon.def.IsRangedWeapon && selPawn.WorkTagIsDisabled(WorkTags.Shooting))
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfShootingLower".Translate(selPawn), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (!selPawn.CanReach(weapon, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "Incapable".Translate().CapitalizeFirst(), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (weapon.IsBurning())
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "BurningLower".Translate(), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			if (selPawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(weapon, selPawn))
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			string text;
			if (!EquipmentUtility.CanEquip(weapon, selPawn, out text, false))
			{
				return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + text.CapitalizeFirst(), null, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
			}
			string text2 = "Equip".Translate(labelShort);
			if (weapon.def.IsRangedWeapon && selPawn.story != null && selPawn.story.traits.HasTrait(TraitDefOf.Brawler))
			{
				text2 += " " + "EquipWarningBrawler".Translate();
			}
			if (EquipmentUtility.AlreadyBondedToWeapon(weapon, selPawn))
			{
				text2 += " " + "BladelinkAlreadyBonded".Translate();
				TaggedString dialogText = "BladelinkAlreadyBondedDialog".Translate(selPawn.Named("PAWN"), weapon.Named("WEAPON"), selPawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
				return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, delegate
				{
					Find.WindowStack.Add(new Dialog_MessageBox(dialogText, null, null, null, null, null, false, null, null, WindowLayer.Dialog));
				}, weapon, Color.white, MenuOptionPriority.High, null, null, 0f, null, null, true, 0), selPawn, weapon, "ReservedBy", null);
			}
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, delegate
			{
				Action equipAction = delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.Equip, weapon);
					job.ignoreForbidden = true;
					selPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
				};
				string personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(weapon, selPawn);
				if (!personaWeaponConfirmationText.NullOrEmpty())
				{
					Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), equipAction, "No".Translate(), null, null, false, null, null, WindowLayer.Dialog));
					return;
				}
				equipAction();
			}, weapon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), selPawn, this, "ReservedBy", null);
		}

		private void SetAllowHauling(bool allow)
		{
			if (this.allowRemovingItems == allow)
			{
				return;
			}
			this.allowRemovingItems = allow;
			base.Map.listerHaulables.RecalculateAllInHaulSource(this);
		}

		private string GetContentsString()
		{
			return "Contents".Translate() + ": " + this.innerContainer.InnerListForReading.Select((Thing a) => a.LabelNoParenthesis).ToCommaList(false, false).CapitalizeFirst();
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString() ?? "";
			if (this.storageGroup != null)
			{
				text += string.Format("{0}: {1} ", "StorageGroupLabel".Translate(), this.storageGroup.RenamableLabel.CapitalizeFirst());
				if (this.storageGroup.MemberCount > 1)
				{
					text += "(" + "NumBuildings".Translate(this.storageGroup.MemberCount).CapitalizeFirst() + ")";
				}
				else
				{
					text += "(" + "OneBuilding".Translate() + ")";
				}
			}
			if (!this.innerContainer.Any)
			{
				return text;
			}
			if (text.Length > 0)
			{
				text += "\n";
			}
			return text + this.GetContentsString();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(this.GetStoreSettings()))
			{
				yield return gizmo2;
			}
			enumerator = null;
			if (this.StorageTabVisible && base.MapHeld != null)
			{
				foreach (Gizmo gizmo3 in StorageGroupUtility.StorageGroupMemberGizmos(this))
				{
					yield return gizmo3;
				}
				enumerator = null;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				yield return new Command_Action
				{
					defaultLabel = "SwapOutfitGizmo".Translate().CapitalizeFirst(),
					defaultDesc = "SwapOutfitDesc".Translate() + "\n\n" + this.GetContentsString(),
					icon = Building_NewOutfitStand.swapOutfitIcon,
					Disabled = !this.innerContainer.Any,
					disabledReason = ((!this.innerContainer.Any) ? "OutfitStandEmpty".Translate().CapitalizeFirst() : null),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters.ForColonist(), delegate(LocalTargetInfo t)
						{
							Pawn pawn;
							if (!t.TryGetPawn(out pawn))
							{
								return;
							}
							if (pawn.Downed)
							{
								Messages.Message("IsIncapped".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.RejectInput, false);
								return;
							}
							if (!pawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Deadly, 1, -1, null, false))
							{
								return;
							}
							this.SetAllowHauling(false);
							pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.UseOutfitStand, this), new JobTag?(JobTag.Misc), false);
						}, null, null, null, true);
					}
				};
				yield return new Command_Toggle
				{
					defaultLabel = "CommandAllowRemovingApparel".Translate(),
					defaultDesc = "CommandAllowRemovingApparelDesc".Translate(),
					hotKey = KeyBindingDefOf.Command_ItemForbid,
					icon = TexCommand.ForbidOff,
					isActive = () => this.allowRemovingItems,
					toggleAction = delegate
					{
						this.SetAllowHauling(!this.allowRemovingItems);
					}
				};
				foreach (Gizmo gizmo4 in GetRenderSelectionGizmos())
				{
					yield return gizmo4;
				}
			}
			yield break;
			yield break;
		}

		private IEnumerable<Gizmo> GetRenderSelectionGizmos()
		{
			yield return new Command_Action
			{
				defaultLabel = "Race: " + (GetSelectedRaceDef() != null ? GetSelectedRaceDef().LabelCap.ToString() : "Default"),
				defaultDesc = "Select race for display.",
				action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					list.Add(new FloatMenuOption("Default", delegate
					{
						selectedRaceDefName = "";
						RecacheGraphics();
					}));
					foreach (ThingDef def in GetRaceOptions())
					{
						ThingDef localDef = def;
						list.Add(new FloatMenuOption(localDef.LabelCap.ToString(), delegate
						{
							selectedRaceDefName = localDef.defName;
							RecacheGraphics();
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
			};

			yield return new Command_Action
			{
				defaultLabel = "Gender: " + selectedGender.ToString(),
				defaultDesc = "Select gender for display.",
				action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					list.Add(new FloatMenuOption("Male", delegate
					{
						selectedGender = Gender.Male;
						RecacheGraphics();
					}));
					list.Add(new FloatMenuOption("Female", delegate
					{
						selectedGender = Gender.Female;
						RecacheGraphics();
					}));
					Find.WindowStack.Add(new FloatMenu(list));
				}
			};

			yield return new Command_Action
			{
				defaultLabel = "Body: " + GetSelectedBodyTypeDef().defName,
				defaultDesc = "Select body type for display.",
				action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (BodyTypeDef def in GetBodyTypeOptions())
					{
						BodyTypeDef localDef = def;
						list.Add(new FloatMenuOption(localDef.defName, delegate
						{
							selectedBodyTypeDefName = localDef.defName;
							RecacheGraphics();
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
			};

			yield return new Command_Action
			{
				defaultLabel = "Head: " + GetSelectedHeadTypeDef().defName,
				defaultDesc = "Select head type for display.",
				action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (HeadTypeDef def in GetHeadTypeOptions())
					{
						HeadTypeDef localDef = def;
						list.Add(new FloatMenuOption(localDef.defName, delegate
						{
							selectedHeadTypeDefName = localDef.defName;
							RecacheGraphics();
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
			};

			yield return new Command_Action
			{
				defaultLabel = "Model Scale: " + this.modelScale.ToString("0.00"),
				defaultDesc = "Adjust stand model scale (excluding base).",
				action = delegate
				{
					int start = Mathf.RoundToInt(this.modelScale * 100f);
					Find.WindowStack.Add(new Dialog_Slider((int val) => "Model Scale: " + (val / 100f).ToString("0.00"), 50, 200, delegate(int val)
					{
						this.modelScale = val / 100f;
					}, start, 1f));
				}
			};

			yield return new Command_Action
			{
				defaultLabel = "Model Z: " + this.modelZOffset.ToString("0.000"),
				defaultDesc = "Adjust stand model Z offset (excluding base).",
				action = delegate
				{
					int start = Mathf.RoundToInt(this.modelZOffset * 1000f);
					Find.WindowStack.Add(new Dialog_Slider((int val) => "Model Z: " + (val / 1000f).ToString("0.000"), -200, 200, delegate(int val)
					{
						this.modelZOffset = val / 1000f;
					}, start, 1f));
				}
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[] { this });
			Scribe_Deep.Look<StorageSettings>(ref this.settings, "settings", new object[] { this });
			Scribe_References.Look<StorageGroup>(ref this.storageGroup, "storageGroup", false);
			Scribe_Values.Look<bool>(ref this.allowRemovingItems, "allowRemovingItems", false, false);
			Scribe_Values.Look(ref this.selectedRaceDefName, "selectedRaceDefName", "");
			Scribe_Values.Look(ref this.selectedBodyTypeDefName, "selectedBodyTypeDefName", "");
			Scribe_Values.Look(ref this.selectedHeadTypeDefName, "selectedHeadTypeDefName", "");
			Scribe_Values.Look(ref this.selectedGender, "selectedGender", Gender.Male);
			Scribe_Values.Look(ref this.modelScale, "modelScale", 1f);
			Scribe_Values.Look(ref this.modelZOffset, "modelZOffset", 0f);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				foreach (Thing thing in this.innerContainer.InnerListForReading)
				{
					if (!(thing is Apparel) && thing.def.IsWeapon)
					{
						this.holdingWeaponCached = true;
						break;
					}
				}
			}
		}

		private static readonly Vector2 baseDrawSize = new Vector2(1.4f, 1.4f);

		private static readonly Vector2 bodyDrawSize = new Vector2(1.2f, 1.2f);

		private static readonly Vector2 bodyChildDrawSize = new Vector2(1.2f, 1.2f);

		private static readonly Vector2 headDrawSize = new Vector2(1.3f, 1.3f);

		private static bool initializedTextures;

		private static Graphic_Multi baseGraphic;

		private static Graphic_Multi bodyGraphic;

		private static Graphic_Multi bodyGraphicChild;

		private static Graphic_Multi headGraphic;

		private static Texture2D swapOutfitIcon;

		private static PawnRenderNodeTagDef rootNodeTagDef;

		private static PawnRenderNodeTagDef RootNodeTagDef
		{
			get
			{
				if (rootNodeTagDef == null)
				{
					rootNodeTagDef = DefDatabase<PawnRenderNodeTagDef>.GetNamedSilentFail("Root");
				}
				return rootNodeTagDef;
			}
		}

		private ThingOwner<Thing> innerContainer;

		private StorageSettings settings;

		private StorageGroup storageGroup;

		private bool allowRemovingItems;

		private string selectedRaceDefName = "";

		private string selectedBodyTypeDefName = "";

		private string selectedHeadTypeDefName = "";

		private Gender selectedGender = Gender.Male;

		private bool cachedApparelRenderInfoSkipHead;

		private readonly List<Building_NewOutfitStand.CachedGraphicRenderInfo> cachedApparelGraphicsHeadgear = new List<Building_NewOutfitStand.CachedGraphicRenderInfo>();

		private readonly List<Building_NewOutfitStand.CachedGraphicRenderInfo> cachedApparelGraphicsNonHeadgear = new List<Building_NewOutfitStand.CachedGraphicRenderInfo>();

		private readonly List<Building_NewOutfitStand.CachedMultiTexRenderInfo> cachedMultiTexApparelHeadgear = new List<Building_NewOutfitStand.CachedMultiTexRenderInfo>();

		private readonly List<Building_NewOutfitStand.CachedMultiTexRenderInfo> cachedMultiTexApparelNonHeadgear = new List<Building_NewOutfitStand.CachedMultiTexRenderInfo>();

		private readonly List<Building_NewOutfitStand.CachedMultiTexRenderInfo> cachedMultiTexStandBody = new List<Building_NewOutfitStand.CachedMultiTexRenderInfo>();

		private readonly List<Building_NewOutfitStand.CachedMultiTexRenderInfo> cachedMultiTexStandHead = new List<Building_NewOutfitStand.CachedMultiTexRenderInfo>();

		private bool useMultiTexBody;

		private bool useMultiTexHead;

		private bool hideOriginalBody;

		private bool hideOriginalHead;

		private Graphic_Multi standBodyGraphicOverride;

		private Graphic_Multi standHeadGraphicOverride;

		private Building_NewOutfitStand.CachedGraphicRenderInfo? cachedHeldWeaponGraphic;

		private bool holdingWeaponCached;

		private float cachedBeauty = -1f;

		private float modelScale = 1f;

		private float modelZOffset;

		private RenderTreeLayout cachedRenderTreeLayout;

		private string cachedRenderTreeRaceDefName = "";

		private readonly List<Building_NewOutfitStand.DrawItem> drawItems = new List<Building_NewOutfitStand.DrawItem>();

		private sealed class RenderTreeLayout
		{
			public readonly Dictionary<PawnRenderNodeTagDef, RenderTreeNodeLayout> nodesByTag = new Dictionary<PawnRenderNodeTagDef, RenderTreeNodeLayout>();

			public void EnsureFallbackNodes(PawnRenderNodeTagDef rootTag)
			{
				int orderIndex = nodesByTag.Count;
				if (rootTag != null && !nodesByTag.ContainsKey(rootTag))
				{
					nodesByTag.Add(rootTag, new RenderTreeNodeLayout(null, Array.Empty<PawnRenderNodeProperties>(), orderIndex++));
				}
				if (!nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.Body))
				{
					nodesByTag.Add(PawnRenderNodeTagDefOf.Body, new RenderTreeNodeLayout(null, Array.Empty<PawnRenderNodeProperties>(), orderIndex++));
				}
				if (!nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.Head))
				{
					nodesByTag.Add(PawnRenderNodeTagDefOf.Head, new RenderTreeNodeLayout(null, Array.Empty<PawnRenderNodeProperties>(), orderIndex++));
				}
				if (!nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.ApparelBody))
				{
					nodesByTag.Add(PawnRenderNodeTagDefOf.ApparelBody, new RenderTreeNodeLayout(null, Array.Empty<PawnRenderNodeProperties>(), orderIndex++));
				}
				if (!nodesByTag.ContainsKey(PawnRenderNodeTagDefOf.ApparelHead))
				{
					nodesByTag.Add(PawnRenderNodeTagDefOf.ApparelHead, new RenderTreeNodeLayout(null, Array.Empty<PawnRenderNodeProperties>(), orderIndex++));
				}
			}
		}

		private sealed class RenderTreeNodeLayout
		{
			public RenderTreeNodeLayout(PawnRenderNodeProperties props, PawnRenderNodeProperties[] ancestors, int order)
			{
				this.props = props;
				this.ancestors = ancestors ?? Array.Empty<PawnRenderNodeProperties>();
				this.order = order;
			}

			public readonly PawnRenderNodeProperties props;

			public readonly PawnRenderNodeProperties[] ancestors;

			public readonly int order;
		}

		private struct NodeTransform
		{
			public NodeTransform(Vector3 offset, Vector3 scale, float layer, int order)
			{
				this.offset = offset;
				this.scale = scale;
				this.layer = layer;
				this.order = order;
			}

			public Vector3 offset;

			public Vector3 scale;

			public float layer;

			public int order;

			public static readonly NodeTransform Default = new NodeTransform(Vector3.zero, Vector3.one, 0f, 0);
		}

		private enum DrawItemType
		{
			Vanilla,
			MultiTex
		}

		private struct DrawItem
		{
			public Building_NewOutfitStand.DrawItemType type;

			public Building_NewOutfitStand.CachedGraphicRenderInfo vanilla;

			public Building_NewOutfitStand.CachedMultiTexRenderInfo multi;

			public Vector3 nodeOffset;

			public Vector3 nodeScale;

			public float layer;

			public int order;
		}

		private struct CachedGraphicRenderInfo
		{
			public CachedGraphicRenderInfo(Graphic graphic, float layer, Vector3 scale, Vector3 positionOffset, bool useHeadOffset = false)
			{
				this.graphic = graphic;
				this.layer = layer;
				this.scale = scale;
				this.positionOffset = positionOffset;
				this.useHeadOffset = useHeadOffset;
			}

			public Graphic graphic;

			public float layer;

			public Vector3 scale;

			public Vector3 positionOffset;

			public bool useHeadOffset;
		}

		private struct CachedMultiTexRenderInfo
		{
			public CachedMultiTexRenderInfo(TextureLevels textureLevels, MultiTexBatch batch, Apparel apparel, float layer, Vector3 scale, Vector3 positionOffset, bool useHeadOffset)
			{
				this.textureLevels = textureLevels;
				this.batch = batch;
				this.apparel = apparel;
				this.layer = layer;
				this.scale = scale;
				this.positionOffset = positionOffset;
				this.useHeadOffset = useHeadOffset;
			}

			public TextureLevels textureLevels;

			public MultiTexBatch batch;

			public Apparel apparel;

			public float layer;

			public Vector3 scale;

			public Vector3 positionOffset;

			public bool useHeadOffset;
		}
	}
}
