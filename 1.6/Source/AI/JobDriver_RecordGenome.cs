using Verse.AI;

namespace ProjectSilverSquad
{
	public class JobDriver_RecordGenome : JobDriver
	{
		private float scanProgress;
		private const float TotalScanningTime = 2000f;


		ThingClass_GenomeImprint GenomeImprint => (ThingClass_GenomeImprint)job.GetTarget(TargetIndex.A).Thing;
		Thing PawnToScan => job.GetTarget(TargetIndex.B).Thing;


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(GenomeImprint, job))
			{
				return pawn.Reserve(PawnToScan, job);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			Toil scanToil = ToilMaker.MakeToil("ScanToil");
			scanToil.initAction = delegate
			{
				if (PawnToScan is Pawn pawnToScan)
				{
					pawn.pather.StopDead();
					PawnUtility.ForceWait(pawnToScan, 15000, null, maintainPosture: true);
				}
			};
			scanToil.AddFinishAction(delegate
			{
				if (PawnToScan is Pawn pawnToScan)
				{
					if (pawnToScan != null && pawnToScan.CurJobDef == JobDefOf.Wait_MaintainPosture)
					{
						pawnToScan.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			});
			scanToil.tickIntervalAction = delegate (int delta)
			{
				scanProgress += 1f * delta;
				if (scanProgress >= TotalScanningTime)
				{
					if (PawnToScan is Pawn pawnToScan)
					{
						GenomeImprint.RecordGenome(pawnToScan);
					}
					else if (PawnToScan is Corpse corpseToScan)
					{
						GenomeImprint.RecordGenome(corpseToScan.InnerPawn);
					}
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
			};
			scanToil.FailOn(() => PawnToScan is Corpse corpse && corpse.GetRotStage() != RotStage.Fresh);
			scanToil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
			scanToil.defaultCompleteMode = ToilCompleteMode.Never;
			scanToil.WithProgressBar(TargetIndex.A, () => scanProgress / TotalScanningTime);
			yield return scanToil;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref scanProgress, "ProjectSilverSquad_JobDriver_RecordGenome_ScanProgress", 0f);
		}
	}
}
