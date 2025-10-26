using Verse.AI;

namespace ProjectSilverSquad
{
	public class JobDriver_CarryIngredientsToVat : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(TargetA, job))
			{
				pawn.Reserve(TargetB, job);
				return true;
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
			yield return Toils_General.WaitWith(TargetIndex.A, 20, true);
			yield return Toils_General.Do(() => (TargetA.Thing as ThingClass_CloningVat).AddIngredient(job.GetTarget(TargetIndex.B).Thing));
		}
	}
}
