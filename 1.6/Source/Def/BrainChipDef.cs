namespace ProjectSilverSquad
{
	public class BrainChipDef : ThingDef
	{
		public List<BrainChipSkillModification> skillMods;
		public List<BrainChipTraitModification> traitMods;
		public float instabilityOffset;
		public float instabilityFactor = 1f;
		public int embryoGrowingTimeTicksOffset;
		public int pawnGrowingTimeTicksOffset;
		public float embryoGrowingTimeFactor = 1f;
		public float pawnGrowingTimeFactor = 1f;


		public BrainChipCategory Category
		{
			get
			{
				BrainChipCategory cat = BrainChipCategory.None;
				if (!skillMods.NullOrEmpty())
					cat |= BrainChipCategory.SkillOnly;
				if (!traitMods.NullOrEmpty())
					cat |= BrainChipCategory.TraitOnly;
				return cat;
			}
		}


		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string error in base.ConfigErrors())
			{
				yield return error;
			}

			if (skillMods.NullOrEmpty()
				&& traitMods.NullOrEmpty()
				&& instabilityOffset == default
				&& instabilityFactor == 1f
				&& embryoGrowingTimeTicksOffset == default
				&& pawnGrowingTimeTicksOffset == default
				&& embryoGrowingTimeFactor == 1f
				&& pawnGrowingTimeFactor == 1f)
			{
				yield return "No settings specified. The chip will not do anything";
			}
			/*else
			{
				if (skillMods.GroupBy(x => x).Any(g => g.Count() > 1))
				{
					yield return "Duplicate skills found in SkillMods list";
				}
				if (traitMods.GroupBy(x => x).Any(g => g.Count() > 1))
				{
					yield return "Duplicate traits found in TraitMods list";
				}
			}*/
		}
	}
}
