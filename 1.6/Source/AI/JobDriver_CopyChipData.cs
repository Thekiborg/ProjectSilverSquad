using Verse.AI;

namespace ProjectSilverSquad
{
	public class JobDriver_CopyChipData : JobDriver
	{
		private const float TotalScanningTime = 2000f;


		private float scanProgress;


		Thing CopyChip => job.GetTarget(TargetIndex.A).Thing;
		Thing TargetChip => job.GetTarget(TargetIndex.B).Thing;


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}


		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			Toil scanToil = ToilMaker.MakeToil("ScanToil");
			scanToil.tickIntervalAction = delegate (int delta)
			{
				scanProgress += 1f * delta;
				if (scanProgress >= TotalScanningTime)
				{
					if (CopyChip is IChipDataCopiable copiable)
					{
						copiable.CopyDataFrom(TargetChip);
					}
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
			};
			scanToil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
			scanToil.defaultCompleteMode = ToilCompleteMode.Never;
			scanToil.WithProgressBar(TargetIndex.A, () => scanProgress / TotalScanningTime);
			yield return scanToil;
		}
	}
}
