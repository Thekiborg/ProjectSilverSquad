using System.Text;

namespace ProjectSilverSquad
{
	public static class CloneUtils
	{
		public static Passion AddTo(this Passion first, Passion second)
		{
			int result = (int)first + (int)second;
			return (Passion)Math.Clamp(result, (int)Passion.None, (int)Passion.Major);
		}


		public static Passion Substract(this Passion first, Passion second)
		{
			int result = (int)first - (int)second;
			return (Passion)Math.Clamp(result, (int)Passion.None, (int)Passion.Major);
		}


		public static bool TryGetHediff(this HediffSet HediffSet, HediffDef def, BodyPartRecord part, out Hediff hediff)
		{
			for (int i = 0; i < HediffSet.hediffs.Count; i++)
			{
				Hediff hd = HediffSet.hediffs[i];
				if (hd.def == def && hd.Part.customLabel == part.customLabel)
				{
					hediff = HediffSet.hediffs[i];
					return true;
				}
			}
			hediff = null;
			return false;
		}


		public static StringBuilder AppendLineIfNotEmpty(this StringBuilder stringBuilder, string value)
		{
			if (value.Length > 0)
			{
				stringBuilder.AppendLine(value);
			}
			return stringBuilder;
		}


		public static string GetAptitudeModsTooltip(SkillRecord skillRecord, Dictionary<BrainChipDef, bool> selectedChips)
		{
			StringBuilder sb = new();
			sb.AppendLineIfNotEmpty(GetBrainChipAptitudeOffsetTooltip(GetAllChipModsFor(skillRecord.def, selectedChips)));
			sb.AppendLineIfNotEmpty(GetGeneAptitudeTooltips(skillRecord));
			sb.AppendLineIfNotEmpty(GetHediffAptitudeTooltips(skillRecord));
			sb.AppendLineIfNotEmpty(GetTraitAptitudeTooltips(skillRecord));
			return sb.ToString();
		}


		public static string GetPassionModsTooltip(SkillRecord skillRecord, Dictionary<BrainChipDef, bool> selectedChips)
		{
			StringBuilder sb = new();
			//sb.AppendLineIfNotEmpty(GetBrainChipPassiontTooltip(GetAllChipModsFor(skillRecord.def, selectedChips)));
			//sb.AppendLineIfNotEmpty(GetGeneAptitudeTooltips(skillRecord));
			//sb.AppendLineIfNotEmpty(GetHediffAptitudeTooltips(skillRecord));
			//sb.AppendLineIfNotEmpty(GetTraitAptitudeTooltips(skillRecord));
			return sb.ToString();
		}


		public static string GetHediffAptitudeTooltips(SkillRecord sk)
		{
			StringBuilder levelTooltip = new();
			foreach (Hediff hediff in sk.Pawn.health.hediffSet.hediffs)
			{
				int num3 = hediff.def.AptitudeFor(sk.def);
				if (num3 != 0)
				{
					levelTooltip.AppendLine(hediff.LabelCap + ": " + num3.ToStringWithSign());
				}
			}
			return levelTooltip.ToString();
		}


		public static string GetTraitAptitudeTooltips(SkillRecord sk)
		{
			StringBuilder levelTooltip = new();
			foreach (Trait trait in sk.Pawn.story.traits.allTraits)
			{
				if (!trait.Suppressed)
				{
					int aptitudeOffset = trait.CurrentData.AptitudeFor(sk.def);
					if (aptitudeOffset != 0)
					{
						levelTooltip.AppendLine(string.Format("{0}: {1}", "TraitLabelWithDesc".Translate(trait.CurrentData.GetLabelFor(sk.Pawn).Named("TRAITLABEL")).CapitalizeFirst(), aptitudeOffset.ToStringWithSign()));
					}
					foreach (SkillGain skillGain in trait.CurrentData.skillGains)
					{
						if (skillGain.skill != sk.def) continue;
						levelTooltip.AppendLine(
							string.Format("{0}: {1}",
									"TraitLabelWithDesc".Translate(trait.CurrentData.GetLabelFor(sk.Pawn).Named("TRAITLABEL")).CapitalizeFirst(),
									skillGain.amount.ToStringWithSign()));
					}
				}
			}
			return levelTooltip.ToString();
		}


		public static string GetGeneAptitudeTooltips(SkillRecord sk)
		{
			StringBuilder levelTooltip = new();
			foreach (Gene gene in sk.Pawn?.genes?.GenesListForReading)
			{
				if (gene.Active)
				{
					int num = gene.def.AptitudeFor(sk.def);
					if (num != 0)
					{
						levelTooltip.AppendLine(string.Format("{0}: {1}", "GeneLabelWithDesc".Translate(gene.def.Named("GENE")).CapitalizeFirst(), num.ToStringWithSign()));
					}
				}
			}
			return levelTooltip.ToString();
		}


		public static string GetBrainChipAptitudeOffsetTooltip(IEnumerable<BrainChipSkillModification> skillBrainChips)
		{
			StringBuilder levelTooltip = new();

			foreach (BrainChipSkillModification mod in skillBrainChips)
			{
				levelTooltip.AppendLine($"{mod.parent.LabelCap}: {mod.skillOffset.ToStringWithSign()}");
			}

			return levelTooltip.ToString();
		}


		/*public static string GetBrainChipPassiontTooltip(IEnumerable<BrainChipSkillModification> skillBrainChips)
		{
			StringBuilder passionTooltip = new();

			foreach (BrainChipSkillModification mod in skillBrainChips)
			{
				if (mod.passionMod == PassionMod.PassionModType.DropAll)
				{
					passionTooltip.AppendLine($"{mod.parent.LabelCap}: {"PassionModDrop".Translate(mod.skillDef)}");
				}
				else if (mod.passionMod == PassionMod.PassionModType.AddOneLevel)
				{
					passionTooltip.AppendLine($"{mod.parent.LabelCap}: {"PassionModAdd".Translate(mod.skillDef)}");
				}
			}

			return passionTooltip.ToString();
		}*/


		public static IEnumerable<BrainChipSkillModification> GetAllChipModsFor(SkillDef def, Dictionary<BrainChipDef, bool> skillMods)
		{
			foreach (KeyValuePair<BrainChipDef, bool> kvp in skillMods)
			{
				if (!kvp.Value) continue;

				foreach (BrainChipSkillModification mod in kvp.Key.skillMods)
				{
					if (mod.skillDef != def) continue;
					yield return mod;
				}
			}
		}
	}
}
