namespace ProjectSilverSquad
{
	public class OutcomeHediffAlteredClone : CloneFailureOutcome
	{
		public float preNoReturnChance;
		public float postNoReturnChance;
		public List<HediffDefAndPart> allowedHediffs;
		public IntRange numberOfHediffs;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
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
				int rand = numberOfHediffs.RandomInRange;

				for (int i = 0; i < rand; i++)
				{
					Log.Message(i);
					var hdAndPart = allowedHediffs.RandomElement();
					BodyPartRecord part = null;
					if (!hdAndPart.wholeBody)
					{
						if (hdAndPart.parts.NullOrEmpty())
						{
							part = clone.RaceProps.body.AllParts.RandomElement();
						}
						else
						{
							part = clone.RaceProps.body.GetPartsWithDef(hdAndPart.parts.RandomElement()).RandomElement();
						}
					}
					clone.health.AddHediff(hdAndPart.hediffDef, part);
				}
				vat.FinishCloning();
			}
		}
	}
}
