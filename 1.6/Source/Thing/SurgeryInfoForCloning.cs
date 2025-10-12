namespace ProjectSilverSquad
{
	public class SurgeryInfoForCloning : IExposable
	{
		private RecipeDef recipeDef;
		private BodyPartRecord bodyPartRecord;
		List<ThingDef> ingredients;

		public RecipeDef RecipeDef => recipeDef;
		public BodyPartRecord Part => bodyPartRecord;
		public List<ThingDef> Ingredients => ingredients;

		public SurgeryInfoForCloning() { }

		public SurgeryInfoForCloning(RecipeDef recipeDef, BodyPartRecord bodyPartRecord, List<ThingDef> ingredients)
		{
			this.recipeDef = recipeDef;
			this.bodyPartRecord = bodyPartRecord;
			this.ingredients = ingredients;
		}


		public void ExposeData()
		{
			Scribe_Defs.Look(ref recipeDef, "ProjectSilverSquad_SurgerInfoForCloning_RecipeDef");
			Scribe_BodyParts.Look(ref bodyPartRecord, "ProjectSilverSquad_SurgeryInfoForCloning_BodyPartRecord");
			Scribe_Collections.Look(ref ingredients, "ProjectSilverSquad_SurgerInfoForCloning_Ingredients", LookMode.Def);
		}
	}
}
