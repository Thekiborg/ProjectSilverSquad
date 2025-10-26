namespace ProjectSilverSquad
{
	public class OutcomeMutateClone : CloneFailureOutcome_Bad
	{
		public List<MutantDef> allowedMutations;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			base.Do(vat, clone);
			if (!ModsConfig.AnomalyActive) return;

			MutantUtility.SetPawnAsMutantInstantly(clone, allowedMutations.RandomElement());
			vat.FinishCloning();
		}
	}
}
