using Verse.AI;

namespace ProjectSilverSquad
{
	internal class JobDriver_EmptyVat : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
			yield return Toils_General.WaitWith(TargetIndex.A, 200, true);
			yield return Toils_General.Do(() => (TargetA.Thing as ThingClass_CloningVat).FinishCloning());
		}
	}
}
