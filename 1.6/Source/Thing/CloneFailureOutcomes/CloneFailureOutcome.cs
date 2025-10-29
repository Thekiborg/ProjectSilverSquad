namespace ProjectSilverSquad
{
	public abstract class CloneFailureOutcome
	{
		public virtual void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			SendLetter("SilverSquad_FailedCloneDesc", clone);
		}


		public virtual void SendLetter(string letterBodyKey, Pawn clone)
		{
			Find.LetterStack.ReceiveLetter("SilverSquad_FailedCloneTitle".Translate(), letterBodyKey.Translate(), LetterDefOf.NegativeEvent);
		}
	}
}
