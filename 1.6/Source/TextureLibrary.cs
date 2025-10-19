namespace ProjectSilverSquad
{
	[StaticConstructorOnStartup]
	public static class TextureLibrary
	{
		public static readonly Texture2D ExtinguishedPassion = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinorGray");
		public static readonly Texture2D potatopic = ContentFinder<Texture2D>.Get("Things/Item/Resource/PlantFoodRaw/Potatoes");
		public static readonly Texture2D CancelLoadingIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
		public static readonly Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
		public static readonly Texture2D SkillBarAptitudePositiveTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.8f, 1f, 0.6f, 0.25f));
		public static readonly Texture2D SkillBarAptitudeNegativeTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.5f, 0.6f, 0.25f));
	}
}
