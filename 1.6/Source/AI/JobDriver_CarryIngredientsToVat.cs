using Verse.AI;

namespace ProjectSilverSquad
{
	public class JobDriver_CarryIngredientsToVat : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(TargetA, job))
			{
				pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
				return true;
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in PickUpIngredient())
			{
				yield return toil;
			}
		}


		private IEnumerable<Toil> PickUpIngredient()
		{
			for (int i = 0; i < job.GetTargetQueue(TargetIndex.B).Count; i++)
			{
				yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, false);
				yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
				yield return Toils_Haul.StartCarryThing(TargetIndex.B);
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
				yield return Toils_General.WaitWith(TargetIndex.A, 20, true);
				yield return Toils_General.Do(() => (TargetA.Thing as ThingClass_CloningVat).AddIngredient(job.GetTarget(TargetIndex.B).Thing));
			}
		}
	}
}
