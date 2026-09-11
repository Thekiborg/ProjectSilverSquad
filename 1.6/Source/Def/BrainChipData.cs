namespace ProjectSilverSquad
{
	public class BrainChipData : IExposable
	{
		public List<BrainChipSkillModification> skillMods;
		public List<BrainChipTraitModification> traitMods;
		public float instabilityOffset;
		public float instabilityFactor = 1f;
		public int embryoGrowingTimeTicksOffset;
		public int pawnGrowingTimeTicksOffset;
		public float embryoGrowingTimeFactor = 1f;
		public float pawnGrowingTimeFactor = 1f;


		public void ExposeData()
		{
			Scribe_Collections.Look(ref skillMods, "ProjectSilverSquad_BrainChipData_skillMods", LookMode.Deep);
			Scribe_Collections.Look(ref traitMods, "ProjectSilverSquad_BrainChipData_traitMods", LookMode.Deep);
			Scribe_Values.Look(ref instabilityOffset, "ProjectSilverSquad_BrainChipData_instabilityOffset");
			Scribe_Values.Look(ref instabilityFactor, "ProjectSilverSquad_BrainChipData_instabilityFactor", 1f);
			Scribe_Values.Look(ref embryoGrowingTimeTicksOffset, "ProjectSilverSquad_BrainChipData_embryoGrowingTimeTicksOffset");
			Scribe_Values.Look(ref pawnGrowingTimeTicksOffset, "ProjectSilverSquad_BrainChipData_pawnGrowingTimeTicksOffset");
			Scribe_Values.Look(ref embryoGrowingTimeFactor, "ProjectSilverSquad_BrainChipData_embryoGrowingTimeFactor", 1f);
			Scribe_Values.Look(ref pawnGrowingTimeFactor, "ProjectSilverSquad_BrainChipData_pawnGrowingTimeFactor", 1f);

		}
	}
}
