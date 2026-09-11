using System.Text;

namespace ProjectSilverSquad
{
	public class ThingClass_BrainChip : ThingWithComps
	{
		private StringBuilder sb = new();
		public BrainChipData data;
		private Color? colorOverride;


		private BrainChipDef Def => def as BrainChipDef;
		public float Instability
		{
			get
			{
				return 0f;
			}
		}


		private Color GetColor()
		{
			if (colorOverride.HasValue)
			{
				return colorOverride.Value;
			}

			var cat = CloneUtils.CategoryFor(this);
			return cat switch
			{
				BrainChipCategory.SkillOnly => Def.generatedSkillChipColor,
				BrainChipCategory.TraitOnly => Def.generatedTraitChipColor,
				BrainChipCategory.Hybrid => Def.generatedHybridChipColor,
				BrainChipCategory.None or _ => Color.white,
			};
		}


		public override void PostMake()
		{
			// Does this run when spawning in by recipe?
			Log.Message("PostMake ran on brain chip");
			data = CloneUtils.RandomizeBrainChipData();
			Log.Message(data is null);
			Log.Message(data.skillMods is null);
			Log.Message(data.traitMods is null);
			base.PostMake();
		}


		public override string GetInspectString()
		{
			sb.Clear();

			if (data.traitMods is not null)
			{
				sb.Append($"{"SilverSquad_CloningVat_TraitModificationTitle".Translate()}: ");
				foreach (var traitMod in data.traitMods)
				{
					sb.Append($"{traitMod.traitDef.DataAtDegree(traitMod.traitDegree).LabelCap}, ");
				}
				sb.AppendLine();
			}

			if (data.skillMods is not null)
			{
				sb.Append($"{"SilverSquad_CloningVat_SkillModificationTitle".Translate()}: ");
				foreach (var skillMod in data.skillMods)
				{
					sb.Append($"{skillMod.skillDef.LabelCap} ({skillMod.skillOffset}), ");
				}
			}
			// Read one by one all fields in brain chip data.
			// Maybe this should be handled by brain chip data.
			sb.AppendLineIfNotEmpty(base.GetInspectString());
			return sb.ToString();
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref data, "ProjectSilverSquad_ThingClass_BrainChip_Data");
		}
	}
}
