namespace ProjectSilverSquad
{
	public class GenomeImprintInformation : IExposable
	{
		private Pawn clone;
		private long originalAge;
		private BodyTypeDef originalBody;
		public Pawn Clone => clone;
		public long OriginalAgeTicks => originalAge;
		public BodyTypeDef OriginalBody => originalBody;


		public GenomeImprintInformation() { }


		public GenomeImprintInformation(Pawn origin)
		{
			Pawn tempPawn = Find.PawnDuplicator.Duplicate(origin);
			PurgeRelations(tempPawn);
			AdjustHediffs(origin, tempPawn);
			RemovePsychologicalTraits(tempPawn);
			ChangeBackstories(tempPawn);
			ChangeSkills(origin, tempPawn);
			if (ModsConfig.IdeologyActive)
			{
				RemoveIdeology(tempPawn);
				ChangeTattoos(tempPawn);
			}
			this.clone = tempPawn;
			originalAge = clone.ageTracker.AgeBiologicalTicks;
			originalBody = clone.story.bodyType;
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
			pawn.story.Childhood = SilverSquad_BackstoryDefOfs.SilverSquad_Backstory_CloneChild;
			pawn.story.Adulthood = SilverSquad_BackstoryDefOfs.SilverSquad_Backstory_CloneAdult;
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
			Scribe_Deep.Look(ref clone, "ProjectSilverSquad_GenomeImprintInformation_Clone");
			Scribe_Values.Look(ref originalAge, "ProjectSilverSquad_GenomeImprintInformation_OriginalAge");
			Scribe_Defs.Look(ref originalBody, "ProjectSilverSquad_GenomeImprintInformation_OriginalBody");
		}
	}
}