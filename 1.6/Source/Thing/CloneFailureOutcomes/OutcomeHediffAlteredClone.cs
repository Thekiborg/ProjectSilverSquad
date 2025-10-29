namespace ProjectSilverSquad
{
	public class OutcomeHediffAlteredClone : CloneFailureOutcome_CloneEffect
	{
		public float preNoReturnChance;
		public float postNoReturnChance;
		public List<HediffDefAndPart> allowedHediffs;
		public IntRange numberOfHediffs;


		public override void Do(ThingClass_CloningVat vat, Pawn clone)
		{
			base.Do(vat, clone);
			if (vat.PastTicksOfNoReturn)
			{
				SpawnPawnWithDefectsOrKill(vat, clone, postNoReturnChance);
			}
			else
			{
				SpawnPawnWithDefectsOrKill(vat, clone, preNoReturnChance);
			}
		}


		public override void SendLetter(string letterBodyKey, Pawn clone)
		{
			base.SendLetter("SilverSquad_FailedCloneDesc_Hediff", clone);
		}


		private void SpawnPawnWithDefectsOrKill(ThingClass_CloningVat vat, Pawn clone, float chance)
		{
			if (!Rand.Chance(chance))
			{
				int rand = numberOfHediffs.RandomInRange;

				for (int i = 0; i < rand; i++)
				{
					var hdAndPart = allowedHediffs.RandomElement();
					Log.Message(hdAndPart.hediffDef);
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
