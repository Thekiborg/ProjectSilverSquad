namespace ProjectSilverSquad
{
	public class CloningSettings : IExposable
	{
		private List<BrainChipDef> brainChipsSkill;
		private List<BrainChipDef> brainChipsTrait;
		private List<SurgeryInfoForCloning> surgeries;
		private Xenogerm xenogerm;

		public List<BrainChipDef> BrainChipsSkill => brainChipsSkill;
		public List<BrainChipDef> BrainChipsTrait => brainChipsTrait;
		public List<SurgeryInfoForCloning> Surgeries => surgeries;
		public Xenogerm Xenogerm => xenogerm;


		public CloningSettings() { }

		public CloningSettings(List<BrainChipDef> brainChipsSkill, List<BrainChipDef> brainChipsTrait, List<SurgeryInfoForCloning> surgeries, Xenogerm xenogerm)
		{
			this.brainChipsSkill = brainChipsSkill;
			this.brainChipsTrait = brainChipsTrait;
			this.surgeries = surgeries;
			this.xenogerm = xenogerm;
		}


		public void ExposeData()
		{
			Scribe_Collections.Look(ref brainChipsSkill, "ProjectSilverSquad_CloningSettings_BrainChipsSkill", LookMode.Def);
			Scribe_Collections.Look(ref brainChipsTrait, "ProjectSilverSquad_CloningSettings_BrainChipTraits", LookMode.Def);
			Scribe_Collections.Look(ref surgeries, "ProjectSilverSquad_CloningSettings_Surgeries", LookMode.Deep);
			Scribe_Deep.Look(ref xenogerm, "ProjectSilverSquad_CloningSettings_Xenogerm");
		}
	}
}
