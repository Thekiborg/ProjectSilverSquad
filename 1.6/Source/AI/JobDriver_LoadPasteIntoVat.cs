using Verse.AI;

namespace ProjectSilverSquad
{
	public class JobDriver_LoadPasteIntoVat : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, canTakeFromInventory: true);
			yield return Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
			yield return Toils_General.WaitWith(TargetIndex.A, 20, true);
			yield return Toils_General.Do(() => (TargetA.Thing as ThingClass_CloningVat).LoadPaste(job.GetTarget(TargetIndex.B).Thing));
		}
	}
}
