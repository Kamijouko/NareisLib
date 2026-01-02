using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NareisLib
{
	public class JobDriver_UseNewOutfitStand : JobDriver
	{
		private Building_NewOutfitStand OutfitStand
		{
			get
			{
				return this.job.targetA.Thing as Building_NewOutfitStand;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref this.duration, "duration", 0);
			Scribe_Collections.Look(ref this.wornApparelToTransferToStand, "wornApparelToTransferToStand", LookMode.Reference);
			Scribe_Collections.Look(ref this.standApparelToTransferToPawn, "standApparelToTransferToPawn", LookMode.Reference);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			if (this.OutfitStand == null)
			{
				this.EndJobWith(JobCondition.Incompletable);
				return;
			}
			this.standApparelToTransferToPawn = new HashSet<Apparel>();
			this.wornApparelToTransferToStand = new HashSet<Apparel>();
			List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
			IEnumerable<Thing> heldItems = this.OutfitStand.HeldItems;
			this.duration = 0;
			foreach (Thing thing in heldItems)
			{
				Apparel apparel = thing as Apparel;
				if (apparel != null && apparel.PawnCanWear(this.pawn) && ApparelUtility.HasPartsToWear(this.pawn, apparel.def)
					&& (!CompBiocodable.IsBiocoded(apparel) || CompBiocodable.IsBiocodedFor(apparel, this.pawn)))
				{
					bool flag = true;
					foreach (Apparel apparel2 in wornApparel)
					{
						if (!ApparelUtility.CanWearTogether(apparel.def, apparel2.def, this.pawn.RaceProps.body))
						{
							if (this.pawn.apparel.IsLocked(apparel2))
							{
								flag = false;
								break;
							}
							this.duration += (int)(apparel2.GetStatValue(StatDefOf.EquipDelay) * 60f);
							this.wornApparelToTransferToStand.Add(apparel2);
						}
					}
					if (flag)
					{
						this.standApparelToTransferToPawn.Add(apparel);
						this.duration += (int)(apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
					}
				}
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = this.duration;
			yield return toil;
			Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
			toil2.AddFinishAction(new Action(this.DoTransfer));
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}

		private void DoTransfer()
		{
			Building_NewOutfitStand outfitStand = this.OutfitStand;
			if (outfitStand == null)
			{
				return;
			}
			foreach (Apparel apparel in this.wornApparelToTransferToStand)
			{
				this.pawn.apparel.Remove(apparel);
			}
			foreach (Apparel apparel2 in this.standApparelToTransferToPawn)
			{
				outfitStand.RemoveApparel(apparel2);
				this.pawn.apparel.Wear(apparel2, true);
				this.pawn.outfits.forcedHandler.SetForced(apparel2, true);
			}
			foreach (Apparel apparel3 in this.wornApparelToTransferToStand)
			{
				outfitStand.AddApparel(apparel3);
			}
			ThingWithComps heldWeapon = outfitStand.HeldWeapon;
			if (heldWeapon != null && JobDriver_UseNewOutfitStand.PawnCanWieldWeapon(heldWeapon, this.pawn) && outfitStand.RemoveHeldWeapon(heldWeapon))
			{
				ThingWithComps thingWithComps;
				this.pawn.equipment.MakeRoomFor(heldWeapon, out thingWithComps);
				this.pawn.equipment.AddEquipment(heldWeapon);
				if (thingWithComps != null)
				{
					thingWithComps.DeSpawn(DestroyMode.Vanish);
					outfitStand.TryAddHeldWeapon(thingWithComps);
				}
			}
		}

		private static bool PawnCanWieldWeapon(Thing weapon, Pawn pawn)
		{
			string text;
			return (!weapon.def.IsWeapon || !pawn.WorkTagIsDisabled(WorkTags.Violent))
				&& (!weapon.def.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting))
				&& pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
				&& (!pawn.IsQuestLodger() || EquipmentUtility.QuestLodgerCanEquip(weapon, pawn))
				&& EquipmentUtility.CanEquip(weapon, pawn, out text, true);
		}

		private int duration;

		private HashSet<Apparel> wornApparelToTransferToStand;

		private HashSet<Apparel> standApparelToTransferToPawn;
	}
}
