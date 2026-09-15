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


		public static BrainChipCategory CategoryFor(ThingClass_BrainChip chip)
		{
			BrainChipCategory cat = BrainChipCategory.None;
			if (chip.data is null)
				return cat;

			if (!chip.data.skillMods.NullOrEmpty())
				cat |= BrainChipCategory.SkillOnly;
			if (!chip.data.traitMods.NullOrEmpty())
				cat |= BrainChipCategory.TraitOnly;
			return cat;
		}


		public static BrainChipData RandomizeBrainChipData()
		{
			BrainChipDef def = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound;
			BrainChipData data = new();

			if (Rand.Chance(def.chanceForHybridGeneratedChip))
			{
				data.skillMods = GenerateRandomSkills();
				data.traitMods = GenerateRandomTraits();
			}
			else if (Rand.Chance(def.chanceForSkillVsTraitChip))
			{
				data.skillMods = GenerateRandomSkills();
			}
			else
			{
				data.traitMods = GenerateRandomTraits();
			}

			if (Rand.Chance(def.chanceForCloningVatParameters))
			{
				GenerateRandomCloningParameters(data);
			}
			return data;
		}


		private static List<BrainChipSkillModification> GenerateRandomSkills()
		{
			int amount = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomSkillCount.RandomInRange;

			if (amount > DefDatabase<SkillDef>.DefCount)
				amount = DefDatabase<SkillDef>.DefCount;

			List<BrainChipSkillModification> mods = [];

			for (int i = 0; i < amount; i++)
			{
				SkillDef randSkill = DefDatabase<SkillDef>.AllDefs.RandomElement();
				if (mods.Any(mod => mod.skillDef == randSkill))
				{
					i--; // repeat this lap
					continue;
				}

				int skillOffset = Rand.Range(-20, 20);
				mods.Add(new(randSkill, skillOffset));
			}

			return mods;
		}


		private static List<BrainChipTraitModification> GenerateRandomTraits()
		{
			int amount = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomTraitCount.RandomInRange;

			if (amount > DefDatabase<TraitDef>.DefCount)
				amount = DefDatabase<TraitDef>.DefCount;

			List<BrainChipTraitModification> mods = [];

			for (int i = 0; i < amount; i++)
			{
				TraitDef randTrait = DefDatabase<TraitDef>.AllDefs.RandomElement();
				if (mods.Any(mod => mod.traitDef == randTrait))
				{
					i--; // repeat this lap
					continue;
				}

				int traitDegree = GetDegreesFor(randTrait).RandomElement();
				mods.Add(new(randTrait, traitDegree));
			}

			return mods;
			static List<int> GetDegreesFor(TraitDef randTrait)
			{
				List<int> degrees = [];

				for (int i = 0; i < randTrait.degreeDatas.Count; i++)
				{
					var DD = randTrait.degreeDatas[i];

					degrees.Add(DD.degree);
				}

				return degrees;
			}
		}


		private static void GenerateRandomCloningParameters(BrainChipData data)
		{
			// For now, 6 parameters.
			// instabilityOffset and instabilityFactor
			// embryoGrowingTimeTicksOffset and pawnGrowingTimeTicksOffset
			// embryoGrowingTimeFactor and pawnGrowingTimeFactor
			int numOfParams = Rand.RangeInclusive(1, 5);
			List<int> availableParams = [1, 2, 3, 4, 5, 6];

			for (int i = 0; i < numOfParams - 1; i++)
			{
				int randParam = availableParams.RandomElement();
				availableParams.Remove(randParam);

				switch (randParam)
				{
					case 1:
						data.instabilityOffset = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomInstabilityOffset.RandomInRange;
						break;
					case 2:
						data.instabilityFactor = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomInstabilityFactor.RandomInRange;
						break;
					case 3:
						data.embryoGrowingTimeTicksOffset = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomEmbryoGrowingTimeTicksOffset.RandomInRange;
						break;
					case 4:
						data.pawnGrowingTimeTicksOffset = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomPawnGrowingTimeTicksOffset.RandomInRange;
						break;
					case 5:
						data.embryoGrowingTimeFactor = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomEmbryoGrowingTimeFactor.RandomInRange;
						break;
					case 6:
						data.pawnGrowingTimeFactor = SilverSquad_ThingDefOfs.SilverSquad_BrainChip_LootFound.randomPawnGrowingTimeFactor.RandomInRange;
						break;
					default:
						break;
				}
			}
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


		public static string GetAptitudeModsTooltip(SkillRecord skillRecord, Dictionary<ThingClass_BrainChip, bool> selectedChips)
		{
			StringBuilder sb = new();
			sb.AppendLineIfNotEmpty(GetBrainChipAptitudeOffsetTooltip(GetAllChipModsFor(skillRecord.def, selectedChips))); // send name to thing here somehow
			sb.AppendLineIfNotEmpty(GetGeneAptitudeTooltips(skillRecord));
			sb.AppendLineIfNotEmpty(GetHediffAptitudeTooltips(skillRecord));
			sb.AppendLineIfNotEmpty(GetTraitAptitudeTooltips(skillRecord));
			return sb.ToString();
		}


		public static string GetPassionModsTooltip(SkillRecord skillRecord, Dictionary<ThingClass_BrainChip, bool> selectedChips)
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
				levelTooltip.AppendLine($"{mod.skillOffset.ToStringWithSign()}");
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


		public static IEnumerable<BrainChipSkillModification> GetAllChipModsFor(SkillDef def, Dictionary<ThingClass_BrainChip, bool> skillMods)
		{
			foreach (KeyValuePair<ThingClass_BrainChip, bool> kvp in skillMods)
			{
				if (!kvp.Value) continue;

				foreach (BrainChipSkillModification mod in kvp.Key.data.skillMods)
				{
					if (mod.skillDef != def) continue;
					yield return mod;
				}
			}
		}
	}
}
