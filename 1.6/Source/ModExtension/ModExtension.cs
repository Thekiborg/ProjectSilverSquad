using System.Linq;

namespace ProjectSilverSquad
{
	public class ModExtension : DefModExtension
	{
		// --- Cloning vat ---
		public GraphicData embryoGraphicData;
		public float maxNutPasteCapacity;
		public float baseNutConsumptionPerDay;
		public int basePawnGrowTimeTicks;
		public int baseEmbryoIncubationTicks;
		public int ticksOfNoReturn;
		public float instabilityPerPeriod;
		public int instabilityPeriod;
#pragma warning disable CS0649
		private List<WeightedCloneFailureOutcomes> outcomes;
#pragma warning restore
		public IEnumerable<WeightedCloneFailureOutcomes> OrderedOutcomes => outcomes.OrderByDescending(outcome => outcome.weight);


		// --- Genome Scanner ---
		public List<IngredientCount> genomeImprintIngredients;
		public int scanTimeTicks;


		// --- Workgivers ---
		public float loadNutPasteThreshold;
	}
}
