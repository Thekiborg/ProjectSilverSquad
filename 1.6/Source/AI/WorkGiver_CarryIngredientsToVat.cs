using Verse.AI;

namespace ProjectSilverSquad
{
	public class WorkGiver_CarryIngredientsToVat : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(SilverSquad_ThingDefOfs.SilverSquad_CloningVat);
		private readonly List<Thing> foundIngredients = [];


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
			foundIngredients.Clear();

			Thing genome = GenClosest.ClosestThingReachable(pawn.Position,
				pawn.Map,
				ThingRequest.ForDef(SilverSquad_ThingDefOfs.SilverSquad_GenomeImprint),
				PathEndMode.OnCell,
				TraverseParms.For(pawn),
				validator: t => t == cloningVat.Settings.GenomeImprint /*&& !t.IsForbidden(pawn.Faction)*/);
			foundIngredients.Add(genome);

			if (cloningVat.Settings.Xenogerm is not null)
			{
				Thing xenogerm = GenClosest.ClosestThingReachable(pawn.Position,
					pawn.Map,
					ThingRequest.ForDef(ThingDefOf.Xenogerm),
					PathEndMode.OnCell,
					TraverseParms.For(pawn),
					validator: t => t == cloningVat.Settings.Xenogerm /*&& !t.IsForbidden(pawn.Faction)*/);
				foundIngredients.Add(xenogerm);
			}

			foreach (var thingDef in cloningVat.WantedIngredients)
			{
				Thing foundIng = GenClosest.ClosestThingReachable(pawn.Position,
					pawn.Map,
					ThingRequest.ForDef(thingDef),
					PathEndMode.Touch,
					TraverseParms.For(pawn)/*,
					validator: t => !t.IsForbidden(pawn.Faction)*/);
				if (foundIng is not null)
				{
					foundIngredients.Add(foundIng);
				}
			}

			return foundIngredients.Count > 0;
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = JobMaker.MakeJob(SilverSquad_JobDefOfs.SilverSquad_CarryIngredientsToVat, t);
			job.count = foundIngredients.Count;
			job.targetQueueB = foundIngredients.ConvertAll<LocalTargetInfo>(t => t);
			return job;
		}
	}
}
