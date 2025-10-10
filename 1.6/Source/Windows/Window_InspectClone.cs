using System.Text;

namespace ProjectSilverSquad
{
	public class Window_InspectClone : Window
	{
		private enum Tab
		{
			Skills,
			Hediffs
		}


		private readonly Pawn clone;
		private static readonly List<TabRecord> tabs = [];
		private Tab selectedTab;
		private Vector2 scrollPos;
		private const float TripleNameHeight = 30f;
		private const float AgeXenotypeAreasHeight = 22f;
		private const float AgeXenotypeAreasExtraWidth = 7.5f;
		private const float AgeXenotypeAreasInnerPadding = AgeXenotypeAreasExtraWidth / 2;
		private const float XenotypeIconSize = 20f;


		public override Vector2 InitialSize => new(480f, 480f);


		public Window_InspectClone(Pawn pawn)
		{
			this.clone = pawn;
			doCloseX = true;
		}


		public override void DoWindowContents(Rect inRect)
		{
			inRect.width -= 7f;
			inRect.yMin += 8f;
			Rect namesRect;
			using (new TextBlock(GameFont.Medium))
			{
				string name = clone.Name.ToStringFull.CapitalizeFirst();
				float nameWidth = Text.CalcSize(name).x;
				namesRect = new(inRect.xMax - nameWidth, inRect.y, nameWidth, TripleNameHeight);
				Widgets.Label(namesRect, name);
			}

			TryDoXenotypeArea(inRect, namesRect, out Rect? xenotypeRectBg);
			DoAgeArea(inRect, namesRect, xenotypeRectBg, out Rect ageRectBg);

			Rect infoRect = new(inRect.xMax - (inRect.width / 2), ageRectBg.yMax + UIUtils.TinyPadding, inRect.width / 2, inRect.yMax - ageRectBg.yMax - UIUtils.TinyPadding);
			infoRect.yMin += TabDrawer.TabHeight + UIUtils.TinyPadding;

			Widgets.DrawMenuSection(infoRect);

			tabs.Clear();
			tabs.Add(new("Skills".Translate(), () =>
			{
				selectedTab = Tab.Skills;
			},
			selectedTab == Tab.Skills));
			tabs.Add(new("Health".Translate(), () =>
			{
				selectedTab = Tab.Hediffs;
			},
			selectedTab == Tab.Hediffs));
			TabDrawer.DrawTabs(infoRect, tabs);

			switch (selectedTab)
			{
				case Tab.Hediffs:
					HealthCardUtility.DrawHediffListing(infoRect, clone, false, UIUtils.TinyPadding);
					break;
				case Tab.Skills:
					DoSkillList(infoRect);
					break;
				default:
					break;
			}

			Rect pawnPortraitRect = new(inRect.x, inRect.y, (inRect.width / 2) - UIUtils.MediumPadding, inRect.width / 2);
			GUI.DrawTexture(pawnPortraitRect, PortraitsCache.Get(clone, pawnPortraitRect.size, Rot4.South, cameraZoom: 1.4f));

			Rect traitsRect = new(inRect.x,
									pawnPortraitRect.yMax + UIUtils.SmallPadding,
									(inRect.width / 2) - UIUtils.MediumPadding,
									inRect.yMax - pawnPortraitRect.yMax - UIUtils.TinyPadding);

			string translatedTraitTitle = "Traits".Translate().AsTipTitle();
			Rect traitRectLabel = new(traitsRect.xMin, traitsRect.yMin, Text.CalcSize(translatedTraitTitle).x, TripleNameHeight);
			Widgets.Label(traitRectLabel, translatedTraitTitle);

			Rect rectForTraitMosaic = new(traitsRect.x, traitRectLabel.yMax, traitsRect.width, traitsRect.height - traitRectLabel.height);
			UIUtils.DoTraitMosaic(rectForTraitMosaic, clone, null);
		}


		private void DoSkillList(Rect rect)
		{
			int skillCount = clone.skills.skills.Count;
			Rect allSkillsRect = new(rect.x, rect.y, rect.width, SkillUI.SkillHeight * skillCount);
			float yPos = 0f;
			try
			{
				Widgets.BeginScrollView(rect, ref scrollPos, allSkillsRect);
				foreach (SkillRecord skill in clone.skills.skills)
				{
					Rect skillRect = new(rect.x, rect.y + yPos, rect.width, SkillUI.SkillHeight);

					if (Mouse.IsOver(skillRect))
					{
						GUI.DrawTexture(skillRect, TexUI.HighlightTex);
					}
					using (new TextBlock(TextAnchor.MiddleLeft))
					{
						Rect labelRect = new(skillRect.x + UIUtils.TinyPadding, skillRect.y, Text.CalcSize(skill.def.skillLabel.CapitalizeFirst()).x, skillRect.height);
						Widgets.Label(labelRect, skill.def.skillLabel.CapitalizeFirst());

						if (skill.passion > Passion.None)
						{
							Texture2D icon = skill.passion == Passion.Major ? SkillUI.PassionMajorIcon : SkillUI.PassionMinorIcon;
							Rect fireRect = new(skillRect.xMax - UIUtils.TinyPadding - UIUtils.PassionIconSize, skillRect.y, UIUtils.PassionIconSize, UIUtils.PassionIconSize);
							GUI.DrawTexture(fireRect, icon);
						}

					}
					if (Mouse.IsOver(rect))
					{
						TooltipHandler.TipRegion(skillRect, new TipSignal(GetSkillDescription(skill), skill.def.GetHashCode() * 389943));
					}
					yPos += SkillUI.SkillHeight;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}


		private static string GetSkillDescription(SkillRecord skill)
		{
			StringBuilder sb = new();
			TaggedString taggedString = skill.def.LabelCap.AsTipTitle();
			if (skill.TotallyDisabled)
			{
				taggedString += " (" + "DisabledLower".Translate() + ")";
			}
			sb.AppendLineTagged(taggedString);
			sb.AppendLineTagged(skill.def.description.Colorize(ColoredText.SubtleGrayColor)).AppendLine();
			if (!skill.TotallyDisabled)
			{
				float statValue = skill.Pawn.GetStatValue(StatDefOf.GlobalLearningFactor);
				float num4 = 1f;
				float num5 = statValue * skill.passion.GetLearningFactor();
				if (skill.def == SkillDefOf.Animals)
				{
					num4 = skill.Pawn.GetStatValue(StatDefOf.AnimalsLearningFactor);
					num5 *= num4;
				}
				sb.AppendLineTagged(("LearningSpeed".Translate() + ": ").AsTipTitle() + num5.ToStringPercent());
				sb.AppendLine("  - " + "StatsReport_BaseValue".Translate() + ": " + 1f.ToStringPercent());
				sb.AppendLine("  - " + skill.passion.GetLabel() + ": x" + skill.passion.GetLearningFactor().ToStringPercent("F0"));
				if (statValue != 1f)
				{
					sb.AppendLine("  - " + StatDefOf.GlobalLearningFactor.LabelCap + ": x" + statValue.ToStringPercent());
				}
				if (Math.Abs(num4 - 1f) > float.Epsilon)
				{
					sb.AppendLine("  - " + StatDefOf.AnimalsLearningFactor.LabelCap + ": x" + num4.ToStringPercent());
				}
				if (statValue != 1f)
				{
					float baseValueFor = StatDefOf.GlobalLearningFactor.Worker.GetBaseValueFor(StatRequest.For(skill.Pawn));
					sb.AppendLine();
					sb.AppendLineTagged((StatDefOf.GlobalLearningFactor.LabelCap + ": ").AsTipTitle() + statValue.ToStringPercent());
					sb.AppendLine("  - " + "StatsReport_BaseValue".Translate() + ": " + baseValueFor.ToStringPercent());
					ListGlobalLearningSpeedOffsets(skill, sb);
					ListGlobalLearningSpeedFactors(skill, sb);
				}
				if (ModsConfig.BiotechActive && skill.Pawn.DevelopmentalStage.Child())
				{
					sb.AppendLine();
					sb.AppendLineTagged("ChildrenLearn".Translate().Colorize(ColoredText.SubtleGrayColor));
				}
			}
			return sb.ToString().TrimEndNewlines();
		}


		private static void ListGlobalLearningSpeedFactors(SkillRecord sk, StringBuilder sb)
		{
			foreach (Hediff hediff in sk.Pawn.health.hediffSet.hediffs)
			{
				if (!hediff.Visible)
				{
					continue;
				}
				HediffStage curStage = hediff.CurStage;
				if (curStage != null)
				{
					float statFactorFromList = curStage.statFactors.GetStatFactorFromList(StatDefOf.GlobalLearningFactor);
					if (!Mathf.Approximately(statFactorFromList, 1f))
					{
						sb.AppendLine("  - " + hediff.LabelCap + ": x" + statFactorFromList.ToStringPercent());
					}
				}
			}
			if (sk.Pawn.story?.traits != null)
			{
				foreach (Trait allTrait in sk.Pawn.story.traits.allTraits)
				{
					if (!allTrait.Suppressed)
					{
						float statFactorFromList2 = allTrait.CurrentData.statFactors.GetStatFactorFromList(StatDefOf.GlobalLearningFactor);
						if (!Mathf.Approximately(statFactorFromList2, 1f))
						{
							sb.AppendLine("  - " + allTrait.CurrentData.LabelCap + ": x" + statFactorFromList2.ToStringPercent());
						}
					}
				}
			}
			if (!ModsConfig.BiotechActive || sk.Pawn.genes == null)
			{
				return;
			}
			foreach (Gene item in sk.Pawn.genes.GenesListForReading)
			{
				if (item.Active)
				{
					float statFactorFromList3 = item.def.statFactors.GetStatFactorFromList(StatDefOf.GlobalLearningFactor);
					if (!Mathf.Approximately(statFactorFromList3, 1f))
					{
						sb.AppendLine("  - " + item.def.LabelCap + ": x" + statFactorFromList3.ToStringPercent());
					}
				}
			}
		}


		private static void ListGlobalLearningSpeedOffsets(SkillRecord sk, StringBuilder sb)
		{
			foreach (Hediff hediff in sk.Pawn.health.hediffSet.hediffs)
			{
				if (!hediff.Visible)
				{
					continue;
				}
				HediffStage curStage = hediff.CurStage;
				if (curStage != null)
				{
					float statOffsetFromList = curStage.statOffsets.GetStatOffsetFromList(StatDefOf.GlobalLearningFactor);
					if (statOffsetFromList != 0f)
					{
						sb.AppendLine("  - " + hediff.LabelCap + ": " + ((statOffsetFromList >= 0f) ? "+" : "") + statOffsetFromList.ToStringPercent());
					}
				}
			}
			if (sk.Pawn.story?.traits != null)
			{
				foreach (Trait allTrait in sk.Pawn.story.traits.allTraits)
				{
					if (!allTrait.Suppressed)
					{
						float statOffsetFromList2 = allTrait.CurrentData.statOffsets.GetStatOffsetFromList(StatDefOf.GlobalLearningFactor);
						if (statOffsetFromList2 != 0f)
						{
							sb.AppendLine("  - " + allTrait.CurrentData.LabelCap + ": " + ((statOffsetFromList2 >= 0f) ? "+" : "") + statOffsetFromList2.ToStringPercent());
						}
					}
				}
			}
			if (!ModsConfig.BiotechActive || sk.Pawn.genes == null)
			{
				return;
			}
			foreach (Gene item in sk.Pawn.genes.GenesListForReading)
			{
				if (item.Active)
				{
					float statOffsetFromList3 = item.def.statOffsets.GetStatOffsetFromList(StatDefOf.GlobalLearningFactor);
					if (statOffsetFromList3 != 0f)
					{
						sb.AppendLine("  - " + item.def.LabelCap + ": " + ((statOffsetFromList3 >= 0f) ? "+" : "") + statOffsetFromList3.ToStringPercent());
					}
				}
			}
		}


		private void DoAgeArea(Rect inRect, Rect namesRect, Rect? xenotypeRectBg, out Rect bgRect)
		{
			string ageText = "AgeIndicator".Translate(clone.ageTracker.AgeNumberString).CapitalizeFirst();
			float ageTextWidth = Text.CalcSize(ageText).x;
			Rect ageRectBg;
			if (xenotypeRectBg is null)
			{
				ageRectBg = new(inRect.xMax - ageTextWidth - AgeXenotypeAreasExtraWidth, namesRect.yMax + UIUtils.SmallPadding, ageTextWidth + AgeXenotypeAreasExtraWidth, AgeXenotypeAreasHeight);
			}
			else
			{
				ageRectBg = new(xenotypeRectBg.Value.xMin - ageTextWidth - AgeXenotypeAreasExtraWidth - UIUtils.TinyPadding, xenotypeRectBg.Value.yMin, ageTextWidth + AgeXenotypeAreasExtraWidth, AgeXenotypeAreasHeight);
			}

			using (new TextBlock(CharacterCardUtility.StackElementBackground))
			{
				GUI.DrawTexture(ageRectBg, BaseContent.WhiteTex);
			}
			if (Mouse.IsOver(ageRectBg))
			{
				Widgets.DrawHighlight(ageRectBg);
			}
			Rect ageRectText = new(ageRectBg.x + AgeXenotypeAreasInnerPadding, ageRectBg.y, ageRectBg.width, ageRectBg.height);
			Widgets.Label(ageRectText, ageText);
			if (Mouse.IsOver(ageRectText))
			{
				TooltipHandler.TipRegion(ageRectText, () => clone.ageTracker.AgeTooltipString, 48943175);
			}
			bgRect = ageRectBg;
		}


		private void TryDoXenotypeArea(Rect inRect, Rect namesRect, out Rect? rect)
		{
			if (clone.genes is not null)
			{
				float xenotypeLabelWidth = Text.CalcSize(clone.genes.XenotypeLabelCap).x;
				Rect xenotypeRectBg = new(inRect.xMax - xenotypeLabelWidth - XenotypeIconSize - AgeXenotypeAreasExtraWidth,
										  namesRect.yMax + UIUtils.SmallPadding,
										  xenotypeLabelWidth + XenotypeIconSize + AgeXenotypeAreasExtraWidth,
										  AgeXenotypeAreasHeight);

				using (new TextBlock(CharacterCardUtility.StackElementBackground))
				{
					GUI.DrawTexture(xenotypeRectBg, BaseContent.WhiteTex);
				}
				if (Mouse.IsOver(xenotypeRectBg))
				{
					Widgets.DrawHighlight(xenotypeRectBg);
				}
				Rect xenotypeIconRect = new(xenotypeRectBg.x, xenotypeRectBg.y, XenotypeIconSize, XenotypeIconSize);
				using (new TextBlock(XenotypeDef.IconColor))
				{
					GUI.DrawTexture(xenotypeIconRect, clone.genes.XenotypeIcon);
				}
				Rect xenotypeLabelRect = new(xenotypeIconRect.xMax,
											xenotypeRectBg.y,
											xenotypeRectBg.width - xenotypeIconRect.width - AgeXenotypeAreasInnerPadding,
											AgeXenotypeAreasHeight);
				Widgets.Label(xenotypeLabelRect, clone.genes.XenotypeLabelCap);
				if (Mouse.IsOver(xenotypeRectBg))
				{
					TooltipHandler.TipRegion(xenotypeRectBg, () =>
						("Xenotype".Translate() + ": " + clone.genes.XenotypeLabelCap)
						.Colorize(ColoredText.TipSectionTitleColor) +
							"\n\n" + clone.genes.XenotypeDescShort +
							"\n\n" + "ViewGenesDesc".Translate(clone.Named("PAWN")).ToString().StripTags()
						.Colorize(ColoredText.SubtleGrayColor), 5416952);
				}
				if (Widgets.ButtonInvisible(xenotypeRectBg))
				{
					Find.WindowStack.Add(new Dialog_ViewGenes(clone));
				}
				rect = xenotypeRectBg;
			}
			else
			{
				rect = null;
			}
		}
	}
}
