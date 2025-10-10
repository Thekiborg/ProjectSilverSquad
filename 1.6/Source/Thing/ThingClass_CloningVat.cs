namespace ProjectSilverSquad
{
	public class ThingClass_CloningVat : ThingWithComps
	{
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
	}
}
