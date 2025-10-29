namespace ProjectSilverSquad
{
	public class OutcomeMutateClone : CloneFailureOutcome_CloneEffect
	{
		public List<MutantDef> allowedMutations;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			base.Do(vat, clone);
			if (!ModsConfig.AnomalyActive || clone.DevelopmentalStage.Child()) return;

			MutantUtility.SetPawnAsMutantInstantly(clone, allowedMutations.RandomElement());
			vat.FinishCloning();
		}


		public override void SendLetter(string letterBodyKey, Pawn clone)
		{
			if (clone.DevelopmentalStage.Child())
				base.SendLetter("SilverSquad_FailedCloneDesc", clone);
			else
				base.SendLetter("SilverSquad_FailedCloneDesc_Mutant", clone);
		}
	}
}
