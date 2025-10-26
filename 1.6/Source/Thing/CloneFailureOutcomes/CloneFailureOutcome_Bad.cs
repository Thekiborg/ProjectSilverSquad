namespace ProjectSilverSquad
{
	public abstract class CloneFailureOutcome_Bad : CloneFailureOutcome
	{
		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			BackstoryDef story = SilverSquad_BackstoryDefOfs.SilverSquad_Backstory_FailedClone;
			clone.story.Adulthood = story;
			foreach (var bsTrait in story.forcedTraits)
			{
				clone.story.traits.GainTrait(new(bsTrait.def, bsTrait.degree, true), true);
			}
			clone.Notify_DisabledWorkTypesChanged();
		}
	}
}
