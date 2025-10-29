namespace ProjectSilverSquad
{
	[StaticConstructorOnStartup]
	public static class TextureLibrary
	{
		public static readonly Texture2D ExtinguishedPassion = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinorGray");
		public static readonly Texture2D CancelLoadingIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
		public static readonly Texture2D CreateCloneIcon = ContentFinder<Texture2D>.Get("ProjectSilverSquad/Icons/CreateClone");
		public static readonly Texture2D PlusSign = ContentFinder<Texture2D>.Get("ProjectSilverSquad/Icons/PlusSign");
		public static readonly Texture2D InspectGenomeIcon = ContentFinder<Texture2D>.Get("ProjectSilverSquad/Icons/InspectGenome");

		public static readonly Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
		public static readonly Texture2D SkillBarAptitudePositiveTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.8f, 1f, 0.6f, 0.25f));
		public static readonly Texture2D SkillBarAptitudeNegativeTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.5f, 0.6f, 0.25f));
	}
}
