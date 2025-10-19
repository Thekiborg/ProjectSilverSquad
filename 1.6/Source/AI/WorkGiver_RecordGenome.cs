using Verse.AI;

namespace ProjectSilverSquad
{
	public class WorkGiver_RecordGenome : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(SilverSquad_ThingDefOfs.SilverSquad_GenomeImprint);


		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is not ThingClass_GenomeImprint genomeImprint)
			{
				return false;
			}
			if (genomeImprint.pawnToScan is null)
			{
				return false;
			}
			if (genomeImprint.pawnToScan.DestroyedOrNull())
			{
				genomeImprint.pawnToScan = null;
				return false;
			}
			if (genomeImprint.pawnToScan is Corpse corpse && corpse.GetRotStage() != RotStage.Fresh)
			{
				genomeImprint.pawnToScan = null;
				return false;
			}
			if (t.IsForbidden(pawn) || t.IsBurning())
			{
				return false;
			}
			if (!pawn.CanReserve(t) || !pawn.CanReserve(genomeImprint.pawnToScan))
			{
				return false;
			}
			return true;
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = JobMaker.MakeJob(SilverSquad_JobDefOfs.SilverSquad_RecordGenome, t, (t as ThingClass_GenomeImprint).pawnToScan);
			job.count = 2;
			return job;
		}
	}
}
