namespace ProjectSilverSquad
{
	public class OutcomeSpawnAnomaly : CloneFailureOutcome
	{
		public List<PawnKindDef> allowedAnomalies;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			if (!ModsConfig.AnomalyActive) return;

			PawnGenerationRequest req = new(allowedAnomalies.RandomElement(), Faction.OfEntities);
			Pawn entity = PawnGenerator.GeneratePawn(req);
			GenSpawn.Spawn(entity, vat.Position, vat.Map);
		}
	}
}
