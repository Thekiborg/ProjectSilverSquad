namespace ProjectSilverSquad
{
	public class OutcomeGeneAlteredClone : CloneFailureOutcome_Bad
	{
		public float preNoReturnChance;
		public float postNoReturnChance;
		//public List<GeneDef> allowedGeneDefs;
		public IntRange numberOfGenes;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			base.Do(vat, clone);
			if (!ModsConfig.BiotechActive) return;

			if (vat.PastTicksOfNoReturn)
			{
				SpawnPawnWithDefectsOrKill(vat, clone, postNoReturnChance);
			}
			else
			{
				SpawnPawnWithDefectsOrKill(vat, clone, preNoReturnChance);
			}
		}


		private void SpawnPawnWithDefectsOrKill(ThingClass_CloningVat vat, Pawn clone, float chance)
		{
			if (!Rand.Chance(chance))
			{
				int rand = numberOfGenes.RandomInRange;

				for (int i = 0; i < rand; i++)
				{
					clone.genes.AddGene(ProjectSilverSquad.PotentiallyNonGameBreakingGenes.RandomElement(), rand % (i + 1) == 1);
				}
				vat.FinishCloning();
			}
		}
	}
}
