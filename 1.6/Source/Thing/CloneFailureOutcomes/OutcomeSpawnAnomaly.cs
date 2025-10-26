namespace ProjectSilverSquad
{
	public class OutcomeSpawnAnomaly : CloneFailureOutcome_Bad
	{
		public List<PawnKindDef> allowedAnomalies;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			base.Do(vat, clone);
			if (!ModsConfig.AnomalyActive) return;

			PawnGenerationRequest req = new(allowedAnomalies.RandomElement(), Faction.OfEntities);
			Pawn entity = PawnGenerator.GeneratePawn(req);
			GenSpawn.Spawn(entity, vat.Position, vat.Map);
		}
	}
}
