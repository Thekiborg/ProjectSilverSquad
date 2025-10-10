namespace ProjectSilverSquad
{
	public class ThingClass_GenomeImprint : ThingWithComps
	{
		public GenomeImprintInformation genome;


		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}

			yield return new Command_Action()
			{
				defaultLabel = "Spawn clone",
				action = () =>
				{
					GenSpawn.Spawn(genome.Clone, Position, Map);
				}
			};

			yield return new Command_Action()
			{
				defaultLabel = "SilverSquad_GenomeImprint_InspectGizmoLabel".Translate(),
				action = () =>
				{
					Find.WindowStack.Add(new Window_InspectClone(genome.Clone));
				}
			};
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref genome, "SilverSquad_Genome");
		}
	}
}
