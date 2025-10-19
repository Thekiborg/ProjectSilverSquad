namespace ProjectSilverSquad
{
	public class ThingClass_GenomeImprint : ThingWithComps
	{
		public override string Label => genome is null ? base.Label : $"{genome?.Clone?.Name?.ToStringShort ?? ""}'s {base.Label}";
		public GenomeImprintInformation genome;
		public Thing pawnToScan;


		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}

			if (genome is not null)
			{
				yield return new Command_Action()
				{
					defaultLabel = "SilverSquad_GenomeImprint_InspectGizmoLabel".Translate(),
					action = () =>
					{
						Find.WindowStack.Add(new Window_InspectClone(genome.Clone));
					}
				};
			}
			else
			{
				yield return new Command_Target()
				{
					defaultLabel = "SilverSquad_GenomeImprint_RecordImprint".Translate(),
					action = target =>
					{
						pawnToScan = target.Thing;
					},
					targetingParams = new()
					{
						canTargetAnimals = false,
						canTargetBuildings = false,
						canTargetItems = false,
						canTargetLocations = false,
						canTargetPlants = false,
						canTargetHumans = true,
						canTargetBloodfeeders = true,
						canTargetCorpses = true,
						canTargetFires = false,
					}
				};
			}
		}


		public void RecordGenome(Pawn pawn)
		{
			pawnToScan = null;
			genome = new(pawn);
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref genome, "SilverSquad_Genome");
			Scribe_References.Look(ref pawnToScan, "SilverSquad_Genome_PawnToScan");
		}
	}
}
