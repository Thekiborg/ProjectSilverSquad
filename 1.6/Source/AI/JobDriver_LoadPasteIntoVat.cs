using Verse.AI;

namespace ProjectSilverSquad
{
	public class JobDriver_LoadPasteIntoVat : JobDriver
	{
		private bool isPasteDispenser;


		public override void Notify_Starting()
		{
			base.Notify_Starting();
			isPasteDispenser = job.GetTarget(TargetIndex.B).Thing is Building_NutrientPasteDispenser;
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref isPasteDispenser, "ProjectSilverSquad_JobDriver_LoadPasteIntoVat_IsPasteDispenser");
		}


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}


		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (isPasteDispenser)
			{
				foreach (Toil toil in DispenserToils())
					yield return toil;
			}
			else
			{
				foreach (Toil toil in NutPasteToils())
					yield return toil;
			}
		}


		private IEnumerable<Toil> DispenserToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
			yield return Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
			yield return Toils_General.WaitWith(TargetIndex.A, 20, true);
			yield return Toils_General.Do(() => (TargetA.Thing as ThingClass_CloningVat).LoadPaste(pawn.carryTracker.CarriedThing));
		}


		private IEnumerable<Toil> NutPasteToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, canTakeFromInventory: true);
			yield return Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
			yield return Toils_General.WaitWith(TargetIndex.A, 20, true);
			yield return Toils_General.Do(() => (TargetA.Thing as ThingClass_CloningVat).LoadPaste(pawn.carryTracker.CarriedThing));
		}
	}
}
