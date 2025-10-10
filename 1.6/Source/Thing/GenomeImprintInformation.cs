namespace ProjectSilverSquad
{
	public class GenomeImprintInformation : IExposable
	{
		private Pawn clone;
		public Pawn Clone => clone;


		public GenomeImprintInformation() { }

		public GenomeImprintInformation(Pawn origin)
		{
			Pawn tempPawn = Find.PawnDuplicator.Duplicate(origin);
			PurgeRelations(tempPawn);
			AdjustHediffs(origin, tempPawn);
			RemovePsychologicalTraits(tempPawn);
			ChangeBackstories(tempPawn);
			ChangeSkills(origin, tempPawn);
			RemoveIdeology(tempPawn);
			ChangeTattoos(tempPawn);
			this.clone = tempPawn;
		}


		private static void PurgeRelations(Pawn pawn)
		{
			pawn.relations?.ClearAllRelations();
		}


		private static void AdjustHediffs(Pawn origin, Pawn clone)
		{
			foreach (Hediff hd in origin.health?.hediffSet?.hediffs)
			{
				if (hd is Hediff_AddedPart or Hediff_Implant)
				{
					Hediff_MissingPart missingBodyPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, clone);
					missingBodyPart.IsFresh = false;
					missingBodyPart.Part = hd.Part;
					clone.health?.hediffSet.AddDirect(missingBodyPart);
				}
			}
		}


		private static void RemovePsychologicalTraits(Pawn pawn)
		{
			var traitSettings = SilverSquad_TraitSettingsDefOfs.SilverSquad_TraitSettings;

			for (int i = pawn.story.traits.allTraits.Count - 1; i >= 0; i--)
			{
				Trait trait = pawn.story.traits.allTraits[i];
				if (traitSettings.traitsArePsychological.Any(setting => setting.traitDef == trait.def && setting.isPsychological))
				{
					pawn.story.traits.RemoveTrait(trait);
				}
			}
		}


		private static void ChangeBackstories(Pawn pawn)
		{
			pawn.story.Childhood = SilverSquad_BackstoryDefOfs.ColonyChild59;
			pawn.story.Adulthood = SilverSquad_BackstoryDefOfs.ColonyChild59;
			pawn.Notify_DisabledWorkTypesChanged();
		}


		private static void RemoveIdeology(Pawn pawn)
		{
			pawn.ideo?.SetIdeo(null);
		}


		private static void ChangeTattoos(Pawn pawn)
		{
			pawn.style.FaceTattoo = null;
			pawn.style?.BodyTattoo = null;
		}


		private static void ChangeSkills(Pawn origin, Pawn clone)
		{
			foreach (SkillRecord record in clone.skills.skills)
			{
				var originalRecord = origin.skills.GetSkill(record.def);
				record.Level = 0;
				record.xpSinceLastLevel = 0;
				record.xpSinceMidnight = 0;
				record.passion = originalRecord.passion;
			}
			clone.Notify_DisabledWorkTypesChanged();
		}


		public void ExposeData()
		{
			Scribe_Deep.Look(ref clone, "SilverSquad_Clone");
		}
	}
}