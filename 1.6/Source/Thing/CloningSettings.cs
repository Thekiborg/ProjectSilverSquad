namespace ProjectSilverSquad
{
	public class CloningSettings : IExposable
	{
		private List<ThingClass_BrainChip> brainChipsSkill;
		private List<ThingClass_BrainChip> brainChipsTrait;
		private List<SurgeryInfoForCloning> surgeries;
		private ThingClass_GenomeImprint genomeImprint;
		private int pawnGrowTicks;
		private int embryoGrowTicks;
		private float instability;
		private Xenogerm xenogerm;


		public List<ThingClass_BrainChip> BrainChipsSkill => brainChipsSkill;
		public List<ThingClass_BrainChip> BrainChipsTrait => brainChipsTrait;
		public List<SurgeryInfoForCloning> Surgeries => surgeries;
		public ThingClass_GenomeImprint GenomeImprint => genomeImprint;
		public float Instability { get => instability; set => instability = value; }
		public Xenogerm Xenogerm => xenogerm;
		public Pawn Clone => GenomeImprint.genome.Clone;
		public int PawnGrowTicks => pawnGrowTicks;
		public int EmbryoGrowTicks => embryoGrowTicks;


		public CloningSettings() { }


		public CloningSettings(List<ThingClass_BrainChip> brainChipsSkill, List<ThingClass_BrainChip> brainChipsTrait, List<SurgeryInfoForCloning> surgeries, Xenogerm xenogerm, ThingClass_GenomeImprint genomeImprint, float instability, int pawnGrowTicks, int embryoGrowTicks)
		{
			this.brainChipsSkill = brainChipsSkill;
			this.brainChipsTrait = brainChipsTrait;
			this.surgeries = surgeries;
			this.xenogerm = xenogerm;
			this.genomeImprint = genomeImprint;
			this.instability = instability;
			this.pawnGrowTicks = pawnGrowTicks;
			this.embryoGrowTicks = embryoGrowTicks;
		}


		public List<Thing> GetUniqueIngredients()
		{
			List<Thing> ingredients = [];

			ingredients.AddRange(BrainChipsSkill);
			ingredients.AddRange(BrainChipsTrait);

			return ingredients;
		}


		public List<ThingDef> GetIngredients()
		{
			List<ThingDef> ingredients = [];

			foreach (var surg in Surgeries)
				ingredients.AddRange(surg.Ingredients);

			return ingredients;
		}


		public void ExposeData()
		{
			Scribe_Collections.Look(ref brainChipsSkill, "ProjectSilverSquad_CloningSettings_BrainChipsSkill", LookMode.Def);
			Scribe_Collections.Look(ref brainChipsTrait, "ProjectSilverSquad_CloningSettings_BrainChipTraits", LookMode.Def);
			Scribe_Collections.Look(ref surgeries, "ProjectSilverSquad_CloningSettings_Surgeries", LookMode.Deep);
			Scribe_Deep.Look(ref xenogerm, "ProjectSilverSquad_CloningSettings_Xenogerm");
			Scribe_References.Look(ref genomeImprint, "ProjectSilverSquad_CloningSettings_GenomeImprint");
			Scribe_Values.Look(ref instability, "ProjectSilverSquad_CloningSettings_Instability");
			Scribe_Values.Look(ref pawnGrowTicks, "ProjectSilverSquad_CloningSettings_PawnGrowTicks");
			Scribe_Values.Look(ref embryoGrowTicks, "ProjectSilverSquad_CloningSettings_EmbryoGrowTicks");
		}
	}
}
