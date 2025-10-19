using Verse.AI;

namespace ProjectSilverSquad
{
	public class WorkGiver_LoadPasteIntoVat : WorkGiver_Scanner
	{
		private ModExtension ModExtension => field ??= def.GetModExtension<ModExtension>();
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(SilverSquad_ThingDefOfs.SilverSquad_CloningVat);
		private Thing foundPaste;


		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is not ThingClass_CloningVat cloningVat)
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
			if ((cloningVat.ModExtension.maxNutPasteCapacity - cloningVat.Nutrition) < ModExtension.loadNutPasteThreshold)
			{
				return false;
			}
			return FindIngredients(pawn);
		}


		private bool FindIngredients(Pawn pawn)
		{
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position,
				pawn.Map,
				ThingRequest.ForDef(ThingDefOf.MealNutrientPaste),
				PathEndMode.ClosestTouch,
				TraverseParms.For(pawn),
				9999f,
				validator: (t) =>
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
				});
			foundPaste = thing;
			return foundPaste is not null;
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			ThingClass_CloningVat cloningVat = t as ThingClass_CloningVat;

			Job job = JobMaker.MakeJob(SilverSquad_JobDefOfs.SilverSquad_LoadPasteIntoVat, cloningVat, foundPaste);
			job.count = (int)((cloningVat.ModExtension.maxNutPasteCapacity - cloningVat.Nutrition) / foundPaste.GetStatValue(StatDefOf.Nutrition));
			return job;
		}
	}
}
