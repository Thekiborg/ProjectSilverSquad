namespace ProjectSilverSquad
{
	public class GameComponent_CloneSkillMods : GameComponent
	{
		public Dictionary<Pawn, List<ThingClass_BrainChip>> AppliedSkillBrainChipsPerPawn = [];
		public Dictionary<Pawn, List<ThingClass_BrainChip>> AppliedTraitBrainChipsPerPawn = [];


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by game")]
		public GameComponent_CloneSkillMods(Game game)
		{
			ProjectSilverSquad.CloneSkillMods = this;
		}
	}
}
