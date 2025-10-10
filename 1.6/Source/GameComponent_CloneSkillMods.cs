namespace ProjectSilverSquad
{
	public class GameComponent_CloneSkillMods : GameComponent
	{
		public Dictionary<Pawn, List<BrainChipDef>> AppliedSkillBrainChipsPerPawn = [];
		public Dictionary<Pawn, List<BrainChipDef>> AppliedBrainChipModsTrait = [];


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by game")]
		public GameComponent_CloneSkillMods(Game game) { }
	}
}
