namespace ProjectSilverSquad
{
	public class CloningSettings : IExposable
	{
		private Dictionary<SkillDef, int> skillLevels;
		private List<BrainChipDef> brainChipsSkill;
		private List<BrainChipDef> brainChipsTrait;
		private List<SurgeryInfoForCloning> surgeries;
		private ThingClass_GenomeImprint genomeImprint;
		private float instability;
		private Xenogerm xenogerm;


		public Dictionary<SkillDef, int> SkillLevels => skillLevels;
		public List<BrainChipDef> BrainChipsSkill => brainChipsSkill;
		public List<BrainChipDef> BrainChipsTrait => brainChipsTrait;
		public List<SurgeryInfoForCloning> Surgeries => surgeries;
		public ThingClass_GenomeImprint GenomeImprint => genomeImprint;
		public float Instability { get => instability; set => instability = value; }
		public Xenogerm Xenogerm => xenogerm;
		public Pawn Clone => GenomeImprint.genome.Clone;


		public CloningSettings() { }


		public CloningSettings(List<BrainChipDef> brainChipsSkill, List<BrainChipDef> brainChipsTrait, List<SurgeryInfoForCloning> surgeries, Xenogerm xenogerm, ThingClass_GenomeImprint genomeImprint, float instability, Dictionary<SkillDef, int> skillLevels)
		{
			this.brainChipsSkill = brainChipsSkill;
			this.brainChipsTrait = brainChipsTrait;
			this.surgeries = surgeries;
			this.xenogerm = xenogerm;
			this.genomeImprint = genomeImprint;
			this.instability = instability;
			this.skillLevels = skillLevels;
		}


		public List<ThingDef> GetIngredients()
		{
			List<ThingDef> ingredients = [];

			ingredients.AddRange(BrainChipsSkill);
			ingredients.AddRange(BrainChipsTrait);
			foreach (var surg in Surgeries)
				ingredients.AddRange(surg.Ingredients);

			return ingredients;
		}


		public void ExposeData()
		{
			Scribe_Collections.Look(ref skillLevels, "ProjectSilverSquad_CloningSettings_SkillLevels", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref brainChipsSkill, "ProjectSilverSquad_CloningSettings_BrainChipsSkill", LookMode.Def);
			Scribe_Collections.Look(ref brainChipsTrait, "ProjectSilverSquad_CloningSettings_BrainChipTraits", LookMode.Def);
			Scribe_Collections.Look(ref surgeries, "ProjectSilverSquad_CloningSettings_Surgeries", LookMode.Deep);
			Scribe_Deep.Look(ref xenogerm, "ProjectSilverSquad_CloningSettings_Xenogerm");
			Scribe_References.Look(ref genomeImprint, "ProjectSilverSquad_CloningSettings_GenomeImprint");
			Scribe_Values.Look(ref instability, "ProjectSilverSquad_CloningSettings_Instability");
		}
	}
}
