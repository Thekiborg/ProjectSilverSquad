using Verse.AI;

namespace ProjectSilverSquad
{
	public class WorkGiver_CarryIngredientsToVat : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(SilverSquad_ThingDefOfs.SilverSquad_CloningVat);
		private Thing foundThing;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is not ThingClass_CloningVat cloningVat)
			{
				return false;
			}
			if (cloningVat.State != VatState.AwaitingIngredients)
			{
				return false;
			}
			if (cloningVat.CompPowerTrader is not null && !cloningVat.CompPowerTrader.PowerOn)
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
			return FindIngredients(pawn, cloningVat);
		}


		private bool FindIngredients(Pawn pawn, ThingClass_CloningVat cloningVat)
		{
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position,
				pawn.Map,
				ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
				PathEndMode.ClosestTouch,
				TraverseParms.For(pawn),
				9999f,
				validator: (t) =>
				{
					if (t == cloningVat.Settings.GenomeImprint)
					{
						return ItemValidator(t, pawn);
					}
					if (t == cloningVat.Settings.Xenogerm)
					{
						return ItemValidator(t, pawn);
					}
					if (cloningVat.WantedIngredients.Contains(t.def))
					{
						return ItemValidator(t, pawn);
					}
					return false;
				});
			foundThing = thing;
			return foundThing is not null;
		}


		private static bool ItemValidator(Thing t, Pawn pawn)
		{
			if (!pawn.CanReserve(t))
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			return true;
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = JobMaker.MakeJob(SilverSquad_JobDefOfs.SilverSquad_CarryIngredientsToVat, t, foundThing);
			job.count = 2;
			return job;
		}
	}
}
