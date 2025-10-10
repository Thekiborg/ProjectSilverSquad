namespace ProjectSilverSquad
{
	[Flags]
	public enum BrainChipCategory
	{
		None = 0,
		SkillOnly = 1 << None,
		TraitOnly = 1 << SkillOnly,
		Hybrid = SkillOnly | TraitOnly
	}
}
