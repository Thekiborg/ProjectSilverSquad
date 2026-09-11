namespace ProjectSilverSquad
{
	public class BrainChipDef : ThingDef
	{
		public Color generatedSkillChipColor;
		public Color generatedTraitChipColor;
		public Color generatedHybridChipColor;

		public float chanceForHybridGeneratedChip;
		/// <summary>
		/// This chance works for both types. For 0.6, it'd be 0.6 for a skill chip and 0.4 for a trait chip.
		/// </summary>
		public float chanceForSkillVsTraitChip;
		public float chanceForCloningVatParameters;
		public IntRange randomTraitCount;
		public IntRange randomSkillCount;

		public FloatRange randomInstabilityOffset;
		public FloatRange randomInstabilityFactor;
		public IntRange randomEmbryoGrowingTimeTicksOffset;
		public IntRange randomPawnGrowingTimeTicksOffset;
		public FloatRange randomEmbryoGrowingTimeFactor;
		public FloatRange randomPawnGrowingTimeFactor;
	}
}
