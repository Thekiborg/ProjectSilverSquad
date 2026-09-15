using System.Text;

namespace ProjectSilverSquad
{
	public class ThingClass_BrainChip : ThingWithComps
	{
		private readonly StringBuilder sb = new();
		public BrainChipData data;


		private BrainChipDef Def => def as BrainChipDef;
		public float Instability
		{
			get
			{
				float total = 0f;

				if (!data.skillMods.NullOrEmpty())
				{
					foreach (var skillMod in data.skillMods)
					{
						total += Mathf.Abs(skillMod.skillOffset) * Def.instabilityPerSkillPoint;
					}
				}
				if (!data.traitMods.NullOrEmpty())
				{
					total += data.traitMods.Count * Def.instabilityPerTrait;
				}
				total += data.instabilityOffset;
				return total;
			}
		}
		public override Color DrawColor
		{
			get => GetColor();
			set => base.DrawColor = value;
		}


		private Color GetColor()
		{
			if (Def.colorOverride.HasValue)
			{
				return Def.colorOverride.Value;
			}

			var cat = CloneUtils.CategoryFor(this);
			return cat switch
			{
				BrainChipCategory.SkillOnly => Def.skillChipColor,
				BrainChipCategory.TraitOnly => Def.traitChipColor,
				BrainChipCategory.Hybrid => Def.hybridChipColor,
				BrainChipCategory.None or _ => Color.white,
			};
		}


		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (Def.data is not null)
			{
				data = Def.data;
			}
			else
			{
				data ??= CloneUtils.RandomizeBrainChipData();
			}
		}


		public override string GetInspectString()
		{
			sb.Clear();

			if (data.traitMods is not null)
			{
				sb.Append($"{"SilverSquad_BrainChip_TraitTitle".Translate()}");
				foreach (var traitMod in data.traitMods)
				{
					sb.Append($"{traitMod.traitDef.DataAtDegree(traitMod.traitDegree).LabelCap}, ");
				}
				sb.AppendLine();
			}

			if (data.skillMods is not null)
			{
				sb.Append($"{"SilverSquad_BrainChip_SkillTitle".Translate()}");
				foreach (var skillMod in data.skillMods)
				{
					sb.Append($"{skillMod.skillDef.LabelCap} ({skillMod.skillOffset}), ");
				}
				sb.AppendLine();
			}
			if (data.instabilityOffset != 0f)
				sb.AppendLine("SilverSquad_BrainChip_instabilityOffsetTitle".Translate(data.instabilityOffset.ToStringWithSign()));
			if (data.instabilityFactor != 1f)
				sb.AppendLine("SilverSquad_BrainChip_instabilityFactor".Translate(data.instabilityFactor.ToString("0.##")));
			if (data.embryoGrowingTimeTicksOffset != 0f)
				sb.AppendLine("SilverSquad_BrainChip_embryoGrowTicksOffset".Translate(GenDate.ToStringTicksToDays(data.embryoGrowingTimeTicksOffset)));
			if (data.pawnGrowingTimeTicksOffset != 0f)
				sb.AppendLine("SilverSquad_BrainChip_pawnGrowTicksOffset".Translate(GenDate.ToStringTicksToDays(data.pawnGrowingTimeTicksOffset)));
			if (data.embryoGrowingTimeFactor != 1f)
				sb.AppendLine("SilverSquad_BrainChip_embryoGrowTicksFactor".Translate(data.embryoGrowingTimeFactor.ToString("0.##")));
			if (data.pawnGrowingTimeFactor != 1f)
				sb.AppendLine("SilverSquad_BrainChip_pawnGrowTicksFactor".Translate(data.pawnGrowingTimeFactor.ToString("0.##")));

			sb.AppendLineIfNotEmpty(base.GetInspectString());
			return sb.ToString();
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref data, "ProjectSilverSquad_ThingClass_BrainChip_Data");
		}


		public void CopyDataFrom(Thing sourceChip)
		{
			Log.Message(sourceChip.GetType());
			Log.Message((sourceChip as ThingClass_BrainChip).data.skillMods.Count);
			var sourceData = (sourceChip as ThingClass_BrainChip).data;
			data = new()
			{
				skillMods = sourceData.skillMods,
				traitMods = sourceData.traitMods,

				instabilityFactor = sourceData.instabilityFactor,
				instabilityOffset = sourceData.instabilityOffset,
				embryoGrowingTimeFactor = sourceData.embryoGrowingTimeFactor,
				embryoGrowingTimeTicksOffset = sourceData.embryoGrowingTimeTicksOffset,
				pawnGrowingTimeFactor = sourceData.pawnGrowingTimeFactor,
				pawnGrowingTimeTicksOffset = sourceData.pawnGrowingTimeTicksOffset
			};
			Log.Message(data.skillMods.Count);
		}


		public bool IsEmpty()
		{
			// It's empty when it's set in xml as <data />, the object is not null but everything else is set as default. Really not ideal but idk how else to do it.
			return data is not null
				&& data.skillMods.NullOrEmpty()
				&& data.traitMods.NullOrEmpty()
				&& data.instabilityOffset == default
				&& data.instabilityFactor == 1f
				&& data.embryoGrowingTimeTicksOffset == default
				&& data.pawnGrowingTimeTicksOffset == default
				&& data.embryoGrowingTimeFactor == 1f
				&& data.pawnGrowingTimeFactor == 1f;
		}
	}
}
