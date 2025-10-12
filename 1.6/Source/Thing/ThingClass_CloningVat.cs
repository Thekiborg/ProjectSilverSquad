namespace ProjectSilverSquad
{
	public class ThingClass_CloningVat : ThingWithComps
	{
		private CloningSettings cloningSettings;
		public CloningSettings Settings { get => cloningSettings; set => cloningSettings = value; }


		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				yield return gizmo;


			yield return new Command_Action()
			{
				defaultLabel = "SilverSquad_CloningVat_StartGizmoLabel".Translate(),
				action = () =>
				{
					Find.WindowStack.Add(new Window_CloningSettings(this));
				}
			};
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref cloningSettings, "ProjectSilverSquad_CloningVat_CloningSettings");
		}
	}
}
