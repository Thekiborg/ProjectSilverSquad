using Verse.AI;

namespace ProjectSilverSquad
{
	public class WorkGiver_EmptyVat : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(SilverSquad_ThingDefOfs.SilverSquad_CloningVat);


		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is not ThingClass_CloningVat cloningVat)
			{
				return false;
			}
			if (cloningVat.State != VatState.Growing)
			{
				return false;
			}
			if (pawn is null || cloningVat.IsForbidden(pawn) || !pawn.CanReserve(cloningVat))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(cloningVat, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (cloningVat.IsBurning())
			{
				return false;
			}
			if (cloningVat.EmbryoIncubationTimeLeft <= 0 && cloningVat.PawnGrowTimeLeft <= 0 && cloningVat.Settings.Surgeries.NullOrEmpty())
			{
				return true;
			}
			return false;
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(SilverSquad_JobDefOfs.SilverSquad_EmptyVat, t);
		}
	}
}
