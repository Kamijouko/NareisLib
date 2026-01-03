using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace NareisLib
{
	public class JobDriver_UseNewOutfitStand_ExchangeAll : JobDriver
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
			Building_NewOutfitStand outfitStand = this.OutfitStand;
			if (outfitStand == null || this.pawn.apparel == null)
			{
				this.EndJobWith(JobCondition.Incompletable);
				return;
			}
			this.standApparelToTransferToPawn = new HashSet<Apparel>();
			this.wornApparelToTransferToStand = new HashSet<Apparel>();
			this.duration = 0;
			foreach (Thing thing in outfitStand.HeldItems)
			{
				Apparel apparel = thing as Apparel;
				if (apparel != null)
				{
					this.standApparelToTransferToPawn.Add(apparel);
					this.duration += (int)(apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
				}
			}
			List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel2 = wornApparel[i];
				if (apparel2 != null)
				{
					this.wornApparelToTransferToStand.Add(apparel2);
					this.duration += (int)(apparel2.GetStatValue(StatDefOf.EquipDelay) * 60f);
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
			if (outfitStand == null || this.pawn.apparel == null)
			{
				return;
			}
			foreach (Apparel apparel in this.wornApparelToTransferToStand)
			{
				this.pawn.apparel.Remove(apparel);
				this.pawn.apparel.Notify_ApparelRemoved(apparel);
			}
			foreach (Apparel apparel2 in this.standApparelToTransferToPawn)
			{
				outfitStand.RemoveApparel(apparel2);
				ForceWear(this.pawn, apparel2);
				if (this.pawn.outfits != null)
				{
					this.pawn.outfits.forcedHandler.SetForced(apparel2, true);
				}
			}
			foreach (Apparel apparel3 in this.wornApparelToTransferToStand)
			{
				outfitStand.AddApparel(apparel3);
			}
		}

		private static void ForceWear(Pawn pawn, Apparel apparel)
		{
			apparel.DeSpawnOrDeselect(DestroyMode.Vanish);
			pawn.apparel.GetDirectlyHeldThings().TryAdd(apparel, false);
			pawn.apparel.Notify_ApparelAdded(apparel);
		}

		private int duration;
		private HashSet<Apparel> wornApparelToTransferToStand;
		private HashSet<Apparel> standApparelToTransferToPawn;
	}
}
