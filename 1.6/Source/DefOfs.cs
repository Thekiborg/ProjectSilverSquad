namespace ProjectSilverSquad
{
#pragma warning disable CA2211
	[DefOf]
	public static class SilverSquad_BackstoryDefOfs
	{
		static SilverSquad_BackstoryDefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SilverSquad_BackstoryDefOfs));
		}

		public static BackstoryDef ColonyChild59;
	}


	[DefOf]
	public static class SilverSquad_ThingDefOfs
	{
		static SilverSquad_ThingDefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SilverSquad_ThingDefOfs));
		}

		public static ThingDef SilverSquad_GenomeImprint;
		public static ThingDef SilverSquad_CloningVat;
	}


	[DefOf]
	public static class SilverSquad_TraitSettingsDefOfs
	{
		static SilverSquad_TraitSettingsDefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SilverSquad_TraitSettingsDefOfs));
		}

		public static TraitSettingsDef SilverSquad_TraitSettings;
	}


	[DefOf]
	public static class SilverSquad_JobDefOfs
	{
		static SilverSquad_JobDefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SilverSquad_JobDefOfs));
		}

		public static JobDef SilverSquad_CarryIngredientsToVat;
		public static JobDef SilverSquad_RecordGenome;
		public static JobDef SilverSquad_LoadPasteIntoVat;
	}
}
