using System.Linq;

namespace ProjectSilverSquad
{
	internal class Window_CloningSettings : Window
	{
		#region constants
		private const float LeftSectionWidth = 310f;
		private const float RightSectionWidth = 500f;
		private const float SectionPadding = 60f;
		private const float BottomPadding = 30f;

		private const float CharacterImprintLeftPadding = 20f;
		private const float CharacterImprintTitleHeight = 100f;
		private const float CharacterImprintPortraitHeight = 350f;
		private const float CharacterTextHeight = 40f;

		private const float SkillTitleHeight = 50f;
		private const float SkillListHeight = 500f;
		private const float SkillLevelRectWidth = 40f;
		private const float TraitTitleHeight = 50f;
		private const float ChipButtonSize = 90f;

		private const float HealthTitleHeight = 50f;
		private const float HealthListHeight = 500f;

		private const float ConfirmExitButtonsSeparation = 50f;
		private const float ButtonPadding = 30f;
		private const float ButtonHeight = 50f;

		#endregion

		private readonly ThingClass_CloningVat cloningVat;

		private Dialog_SelectBrainChipSkill selectBrainChipSkill;
		private Dialog_SelectBrainChipTrait selectBrainChipTrait;
		private Dialog_SelectSurgery selectSurgery;

		public Dictionary<BodyPartRecord, List<Hediff>> originalHediffs = [];
		private readonly Dictionary<PawnCapacityDef, string> initialCloneCapacities = [];

		// Results to be passed to the vat
		public Dictionary<BrainChipDef, bool> selectedSkillChips = [];
		public Dictionary<BrainChipDef, bool> selectedTraitChips = [];
		public Dictionary<(RecipeDef, BodyPartRecord), bool> selectedSurgeries = [];
		public Xenogerm xenogerm;

		private Pawn backerPawn;
		private ThingClass_GenomeImprint imprint;
		private Vector2 scrollPosition;
		public (XenotypeDef, string, XenotypeIconDef, List<Gene>) preXenogermInfo;

		private float Instability
		{
			get
			{
				float instability = 0f;
				foreach (var kvp in selectedSkillChips)
				{
					if (kvp.Value)
						instability += kvp.Key.instabilityOffset;
				}
				foreach (var kvp in selectedTraitChips)
				{
					if (kvp.Value)
						instability += kvp.Key.instabilityOffset;
				}
				return Mathf.Min(instability, 100);
			}
		}

		private (int embryoTicks, int pawnTicks) TicksOfWork
		{
			get
			{
				int embryoTicks = cloningVat.ModExtension.baseEmbryoIncubationTicks;
				float embryoTicksFactor = 1f;
				int pawnTicks = cloningVat.ModExtension.basePawnGrowTimeTicks;
				float pawnTicksFactor = 1f;
				foreach (var kvp in selectedSkillChips)
				{
					if (kvp.Value)
					{
						embryoTicks += kvp.Key.embryoGrowingTimeTicksOffset;
						embryoTicksFactor *= kvp.Key.embryoGrowintTimeFactor;
						pawnTicks += kvp.Key.pawnGrowingTimeTicksOffset;
						pawnTicksFactor *= kvp.Key.pawnGrowingTimeFactor;
					}
				}
				foreach (var kvp in selectedTraitChips)
				{
					if (kvp.Value)
					{
						embryoTicks += kvp.Key.embryoGrowingTimeTicksOffset;
						embryoTicksFactor *= kvp.Key.embryoGrowintTimeFactor;
						pawnTicks += kvp.Key.pawnGrowingTimeTicksOffset;
						pawnTicksFactor *= kvp.Key.pawnGrowingTimeFactor;
					}
				}
				return ((int)(embryoTicks * embryoTicksFactor), (int)(pawnTicks * pawnTicksFactor));
			}
		}

		private Dialog_SelectBrainChipSkill Dialog_SelectBrainChipSkill
		{
			get
			{
				selectBrainChipSkill ??= new(this, cloningVat.Map);
				return selectBrainChipSkill;
			}
		}

		private Dialog_SelectBrainChipTrait Dialog_SelectBrainChipTrait
		{
			get
			{
				selectBrainChipTrait ??= new(this, cloningVat.Map);
				return selectBrainChipTrait;
			}
		}

		private Dialog_SelectSurgery Dialog_SelectSurgery
		{
			get
			{
				selectSurgery ??= new(this, cloningVat.Map);
				return selectSurgery;
			}
		}
		public override Vector2 InitialSize => new(1400, 800);
		public Pawn PreviewClone => backerPawn;


		public Window_CloningSettings(ThingClass_CloningVat cloningVat)
		{
			this.cloningVat = cloningVat;
			forcePause = true;
		}


		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.Layout) return;

			Widgets.DrawMenuSection(inRect);

			Rect leftSideRect = new(inRect.x, inRect.y, LeftSectionWidth, inRect.height);
			DoLeftSide(leftSideRect);

			Rect rightSideRect = new(inRect.xMax - RightSectionWidth, inRect.y, RightSectionWidth - SectionPadding, inRect.height);
			DoRightSide(rightSideRect);

			Rect middleSideRect = new(leftSideRect.xMax + SectionPadding, inRect.y, rightSideRect.xMin - SectionPadding - SectionPadding - leftSideRect.xMax, inRect.height);
			DoMiddleSide(middleSideRect);
		}


		private void DoLeftSide(Rect rect)
		{
			Rect contentRect = new(rect.x + CharacterImprintLeftPadding, rect.y, rect.width - (CharacterImprintLeftPadding * 2), rect.height);


			Rect sideTitle = new(contentRect.x, contentRect.y, contentRect.width, CharacterImprintTitleHeight);
			using (new TextBlock(GameFont.Medium, TextAnchor.LowerLeft))
			{
				Widgets.Label(sideTitle, "SilverSquad_CloningVat_CharacterImprint_Title".Translate());
			}


			Rect portraitRect = new(contentRect.x, sideTitle.yMax, contentRect.width, CharacterImprintPortraitHeight);
			Widgets.DrawWindowBackground(portraitRect);
			if (Mouse.IsOver(portraitRect))
			{
				Widgets.DrawHighlight(portraitRect);
			}
			if (PreviewClone is not null)
			{
				if (Mouse.IsOver(portraitRect))
				{
					TooltipHandler.TipRegion(portraitRect, new TipSignal("SilverSquad_CloningVat_CharacterImprint_PortraitTooltip".Translate(), 11223311));
				}
				GUI.DrawTexture(portraitRect, PortraitsCache.Get(PreviewClone, portraitRect.size, Rot4.South, cameraZoom: 1.3f));
				if (Widgets.ButtonInvisible(portraitRect))
				{
					DoImprintFloatMenu();
				}


				Rect nameRowRect = new(contentRect.x, portraitRect.yMax, portraitRect.width, CharacterTextHeight);
				using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft))
				{
					string cloneNameTranslated = "SilverSquad_CloningVat_CharacterImprint_Name".Translate(PreviewClone).AsTipTitle();
					float textWidth = Text.CalcSize(cloneNameTranslated).x;
					if (textWidth > nameRowRect.width)
						nameRowRect.height *= 2;

					if (Mouse.IsOver(nameRowRect))
					{
						Widgets.DrawHighlight(nameRowRect);
					}
					Rect nameRect = new(nameRowRect);
					nameRect.x += UIUtils.TinyPadding;

					Widgets.Label(nameRect, cloneNameTranslated);
					if (Widgets.ButtonInvisible(nameRowRect))
					{
						NameFilter visibleNames = NameFilter.First | NameFilter.Nick | NameFilter.Last;
						Find.WindowStack.Add(new Dialog_NamePawn(PreviewClone, visibleNames, visibleNames, null));
					}
				}


				Rect ageRowRect = new(contentRect.x, nameRowRect.yMax, nameRowRect.width, CharacterTextHeight);
				if (Mouse.IsOver(ageRowRect))
				{
					Widgets.DrawHighlight(ageRowRect);
				}
				using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft))
				{
					Rect ageRect = new(ageRowRect);
					ageRect.x += UIUtils.TinyPadding;
					Widgets.Label(ageRect, "SilverSquad_CloningVat_CharacterImprint_Age".Translate(PreviewClone.ageTracker.AgeNumberString));
				}


				if (PreviewClone.genes is not null)
				{
					if (ModsConfig.BiotechActive)
					{
						Rect xenoRect = new(contentRect.x, ageRowRect.yMax, ageRowRect.width, CharacterTextHeight);
						if (Mouse.IsOver(xenoRect))
						{
							Widgets.DrawHighlight(xenoRect);
						}
						Rect xenoIconRect = new(xenoRect.x, xenoRect.y, xenoRect.height, xenoRect.height);
						using (new TextBlock(XenotypeDef.IconColor))
						{
							GUI.DrawTexture(xenoIconRect, PreviewClone.genes.XenotypeIcon);
						}
						using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft))
						{
							Rect xenoLabelRect = new(xenoIconRect.xMax, xenoRect.y, xenoRect.width - xenoIconRect.width, xenoRect.height);
							Widgets.Label(xenoLabelRect, PreviewClone.genes.XenotypeLabelCap);
						}
						if (Mouse.IsOver(xenoRect))
						{
							TooltipHandler.TipRegion(xenoRect, () =>
								("Xenotype".Translate() + ": " + PreviewClone.genes.XenotypeLabelCap)
								.Colorize(ColoredText.TipSectionTitleColor) +
									"\n\n" + PreviewClone.genes.XenotypeDescShort +
									"\n\n" + "ViewGenesDesc".Translate(PreviewClone.Named("PAWN")).ToString().StripTags()
								.Colorize(ColoredText.SubtleGrayColor), 5416952);
						}
						if (Widgets.ButtonInvisible(xenoRect))
						{
							Find.WindowStack.Add(new Dialog_ViewGenes(PreviewClone));
						}
					}
				}
			}
			else
			{
				Rect plusSignRect = new(portraitRect.width / 2, portraitRect.yMin, ChipButtonSize, ChipButtonSize);
				GUI.DrawTexture(plusSignRect, TextureLibrary.PlusSign);

				Widgets.DrawBox(plusSignRect);
				//Log.Message($"{plusSignRect.x} - {plusSignRect.xMin}"); HERE
				//Log.Message($"{portraitRect.width} - {portraitRect.width / 2}");
				//Log.Message(contentRect.width);

				TooltipHandler.TipRegion(portraitRect, "SilverSquad_CloningVat_CharacterImprint_PortraitTooltip".Translate());
				if (Widgets.ButtonInvisible(portraitRect))
				{
					DoImprintFloatMenu();
				}
			}
		}


		private void DoRightSide(Rect rect)
		{
			Rect healthTitle = new(rect.x, rect.y, rect.width, SkillTitleHeight);
			using (new TextBlock(GameFont.Medium, TextAnchor.LowerLeft))
			{
				Widgets.Label(healthTitle, "SilverSquad_CloningVat_HealthTitle".Translate());
			}
			Rect healthListRect = new(rect.x, healthTitle.yMax, rect.width, HealthListHeight);
			Rect capsRect = new(healthListRect.x,
								healthListRect.y + healthTitle.height + UIUtils.TinyPadding,
								healthListRect.width * 5 / 10,
								healthListRect.height - HealthTitleHeight - UIUtils.TinyPadding);
			Widgets.DrawWindowBackground(capsRect);
			Rect capacityLabelRect;
			using (new TextBlock(GameFont.Medium))
			{
				string capacityLabel = "SilverSquad_CloningVat_CapacityTitle".Translate();
				capacityLabelRect = new(capsRect.xMin + UIUtils.TinyPadding, capsRect.yMin, capsRect.width, Text.CalcSize(capacityLabel).y);
				Widgets.Label(capacityLabelRect, capacityLabel);
			}
			if (PreviewClone is not null)
			{
				Rect capacityReadoutRect = new(capacityLabelRect.xMin,
												capacityLabelRect.yMax + UIUtils.MediumPadding,
												capacityLabelRect.width - (UIUtils.TinyPadding * 2),
												capsRect.height - capacityLabelRect.height - (UIUtils.MediumPadding * 2));

				Rect allCapsRect = new(capacityReadoutRect.xMin,
						capacityReadoutRect.yMin,
						capacityReadoutRect.width,
						(SkillUI.SkillHeight + UIUtils.TinyPadding) * ProjectSilverSquad.AllHumanlikeCapacities.Count);

				DoCapacityList(ProjectSilverSquad.AllHumanlikeCapacities, capacityReadoutRect, allCapsRect);
			}


			Rect surgeryButtonBgRect = new(capsRect.x,
				healthListRect.y,
				capsRect.width,
				capsRect.yMin + UIUtils.TinyPadding - (healthTitle.yMax + UIUtils.TinyPadding) - UIUtils.TinyPadding);
			Widgets.DrawWindowBackground(surgeryButtonBgRect);
			Rect surgeryButtonRect = new(surgeryButtonBgRect.xMin + UIUtils.MediumPadding,
										surgeryButtonBgRect.yMin + UIUtils.TinyPadding,
										surgeryButtonBgRect.width - (UIUtils.MediumPadding * 2),
										surgeryButtonBgRect.height - (UIUtils.TinyPadding * 2));

			if (Widgets.ButtonText(surgeryButtonRect, "MedicalOperationsShort".Translate()))
			{
				if (PreviewClone is not null)
				{
					Find.WindowStack.Add(Dialog_SelectSurgery);
				}
			}


			Rect hediffsRect = new(capsRect.xMax + UIUtils.TinyPadding, healthListRect.y, healthListRect.width - capsRect.width - UIUtils.TinyPadding, healthListRect.height);
			Widgets.DrawWindowBackground(hediffsRect);
			Rect hediffReadoutRect = new(hediffsRect.xMin, capsRect.yMin, hediffsRect.width, hediffsRect.yMax - capsRect.yMin);
			Rect hediffsLabel = new(hediffsRect.xMin + UIUtils.TinyPadding, hediffsRect.yMin, hediffsRect.width, hediffReadoutRect.yMin - hediffsRect.yMin);
			using (new TextBlock(GameFont.Medium))
			{
				Widgets.Label(hediffsLabel, "SilverSquad_CloningVat_HediffTitle".Translate());
			}
			if (PreviewClone is not null)
			{
				HealthCardUtility.DrawHediffListing(hediffReadoutRect, PreviewClone, false, UIUtils.TinyPadding);
			}


			Rect confirmExitRect = new(rect.x, healthListRect.yMax + ConfirmExitButtonsSeparation, rect.width, rect.yMax - healthListRect.yMax - ConfirmExitButtonsSeparation - BottomPadding);

			float buttonWidth = (confirmExitRect.width - (ButtonPadding * 3)) / 2;
			float buttonY = (confirmExitRect.height / 2) - (ButtonHeight / 2);
			Rect confirmButtonRect = new(confirmExitRect.x + ButtonPadding, confirmExitRect.y + buttonY, buttonWidth, ButtonHeight);
			Rect cancelButtonRect = new(confirmButtonRect.xMax + ButtonPadding, confirmExitRect.y + buttonY, buttonWidth, ButtonHeight);
			using (new TextBlock(GameFont.Medium))
			{
				if (Widgets.ButtonText(confirmButtonRect, "SilverSquad_CloningVat_Confirm".Translate()))
				{
					List<BrainChipDef> brainChipsSkill = [.. selectedSkillChips.Where(kvp => kvp.Value).Select(kvp => kvp.Key)];
					List<BrainChipDef> brainChipsTraits = [.. selectedTraitChips.Where(kvp => kvp.Value).Select(kvp => kvp.Key)];

					List<SurgeryInfoForCloning> surgeries = [];
					foreach (var kvp in selectedSurgeries)
					{
						if (!kvp.Value) continue;

						List<ThingDef> ingredients = [];
						foreach (IngredientCount ic in kvp.Key.Item1.ingredients)
						{
							if (ic.filter.AnyAllowedDef.thingCategories.Contains(ThingCategoryDefOf.Medicine)) continue;
							for (int i = 0; i < ic.CountFor(kvp.Key.Item1); i++)
							{
								ingredients.Add(ic.filter.AnyAllowedDef);
							}
						}
						surgeries.Add(new SurgeryInfoForCloning(kvp.Key.Item1, kvp.Key.Item2, ingredients));
					}
					Dictionary<SkillDef, int> skillLevels = [];
					foreach (var skill in PreviewClone.skills.skills)
					{
						skillLevels.TryAdd(skill.def, skill.Level);
					}

					cloningVat.StartCloning(new(brainChipsSkill, brainChipsTraits, surgeries, xenogerm, imprint, Instability, skillLevels, TicksOfWork.pawnTicks, TicksOfWork.embryoTicks));
					Close();
				}
				using (new TextBlock(ColorLibrary.Red))
				{
					if (Widgets.ButtonText(cancelButtonRect, "SilverSquad_CloningVat_Cancel".Translate()))
					{
						Close();
					}
				}
			}

			Rect instabilityRect = new(confirmButtonRect.xMin, confirmExitRect.yMin, cancelButtonRect.xMax - confirmButtonRect.xMin, confirmButtonRect.yMin - confirmExitRect.yMin);
			instabilityRect.width /= 2;
			using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, ColorLibrary.RedReadable))
			{
				Widgets.Label(instabilityRect, "SilverSquad_CloningVat_Instability".Translate(Instability.ToStringPercent("F0")));
			}

			Rect daysRect = new(instabilityRect.xMax, instabilityRect.y, instabilityRect.width, instabilityRect.height);
			using (new TextBlock(GameFont.Medium, TextAnchor.MiddleRight))
			{
				int totalTicks = TicksOfWork.pawnTicks + TicksOfWork.embryoTicks;
				Widgets.Label(daysRect, "SilverSquad_CloningVat_DaysLeft".Translate(GenDate.TicksToDays(totalTicks)));
			}
		}


		private void DoCapacityList(List<PawnCapacityDef> capacities, Rect capacityReadoutRect, Rect allCapsRect)
		{
			try
			{
				Widgets.BeginScrollView(capacityReadoutRect, ref scrollPosition, allCapsRect, false);

				float yPos = 0;
				foreach (PawnCapacityDef cap in capacities)
				{
					if (!PawnCapacityUtility.BodyCanEverDoCapacity(PreviewClone.RaceProps.body, cap)) continue;

					Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(PreviewClone, cap);

					Rect capRect = new(capacityReadoutRect.x, capacityReadoutRect.yMin + yPos, capacityReadoutRect.width, SkillUI.SkillHeight);
					string capLabel = cap.GetLabelFor(PreviewClone).CapitalizeFirst();
					Rect capLabelRect = new(capRect.x, capRect.y, Text.CalcSize(capLabel).x, capRect.height);
					string capValue = $"{initialCloneCapacities[cap]} ({efficiencyLabel.First})";
					float capValueWidth = Text.CalcSize(capValue).x;
					Rect capValueRect = new(capRect.xMax - capValueWidth, capRect.y, capValueWidth, capRect.height);
					Widgets.Label(capLabelRect, capLabel);
					using (new TextBlock(efficiencyLabel.Second))
					{
						Widgets.Label(capValueRect, capValue);
					}
					if (Mouse.IsOver(capRect))
					{
						Widgets.DrawHighlight(capRect);
						TooltipHandler.TipRegion(capRect, HealthCardUtility.GetPawnCapacityTip(PreviewClone, cap));
					}

					yPos += capRect.height + UIUtils.TinyPadding;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}


		private void DoMiddleSide(Rect rect)
		{
			SkillMenu(rect, out Rect skillMenuRect);
			Rect traitTitleRect = new(rect.x, skillMenuRect.yMax, rect.width, TraitTitleHeight);
			TraitMenu(rect, traitTitleRect);
		}


		private void TraitMenu(Rect rect, Rect traitTitleRect)
		{
			using (new TextBlock(GameFont.Medium, TextAnchor.LowerLeft))
			{
				Widgets.Label(traitTitleRect, "SilverSquad_CloningVat_TraitModificationTitle".Translate());
			}


			Rect traitListRect = new(rect.x, traitTitleRect.yMax, rect.width, rect.yMax - traitTitleRect.yMax - BottomPadding);
			Widgets.DrawWindowBackground(traitListRect);
			Rect chipButtonRect = new(traitListRect.xMin + UIUtils.MediumPadding, traitListRect.y + (traitListRect.height / 2) - (ChipButtonSize / 2), ChipButtonSize, ChipButtonSize);
			using (new TextBlock(CharacterCardUtility.StackElementBackground))
			{
				GUI.DrawTexture(chipButtonRect, BaseContent.WhiteTex);
				if (Widgets.ButtonImage(chipButtonRect, TextureLibrary.PlusSign))
				{
					if (PreviewClone is not null)
					{
						Find.WindowStack.Add(Dialog_SelectBrainChipTrait);
					}
				}
			}
			Rect traitMosaicRect = new(chipButtonRect.xMax + UIUtils.MediumPadding,
										chipButtonRect.yMin - UIUtils.TinyPadding,
										traitListRect.xMax - chipButtonRect.xMax - (UIUtils.MediumPadding * 2),
										chipButtonRect.height + (UIUtils.TinyPadding * 2));

			if (PreviewClone is not null)
			{
				UIUtils.DoTraitMosaic(traitMosaicRect, PreviewClone, selectedTraitChips);
			}
		}


		private void SkillMenu(Rect rect, out Rect menuRect)
		{
			Rect skillTitleRect = new(rect.x, rect.y, rect.width, SkillTitleHeight);
			using (new TextBlock(GameFont.Medium, TextAnchor.LowerLeft))
			{
				Widgets.Label(skillTitleRect, "SilverSquad_CloningVat_SkillModificationTitle".Translate());
			}


			Rect skillMenuRect = new(rect.x, skillTitleRect.yMax, rect.width, SkillListHeight);
			menuRect = skillMenuRect;
			Widgets.DrawWindowBackground(skillMenuRect);
			Rect chipButtonRect = new(skillMenuRect.xMin + UIUtils.MediumPadding, skillMenuRect.yMin + UIUtils.MediumPadding, ChipButtonSize, ChipButtonSize);
			using (new TextBlock(CharacterCardUtility.StackElementBackground))
			{
				GUI.DrawTexture(chipButtonRect, BaseContent.WhiteTex);
				if (Widgets.ButtonImage(chipButtonRect, TextureLibrary.PlusSign))
				{
					if (PreviewClone is not null)
					{
						Find.WindowStack.Add(Dialog_SelectBrainChipSkill);
					}
				}
			}
			if (PreviewClone is not null)
			{
				Rect skillsRect = new(chipButtonRect.xMin,
								chipButtonRect.yMax + UIUtils.MediumPadding,
								rect.width - UIUtils.MediumPadding - GenUI.ScrollBarWidth,
								skillMenuRect.height - chipButtonRect.height - (UIUtils.MediumPadding * 3));

				Rect allSkillsRect = new(skillsRect.xMin,
										skillsRect.yMin,
										skillsRect.width,
										(SkillUI.SkillHeight + UIUtils.TinyPadding) * PreviewClone.skills.skills.Count);

				DoSkillRecordList(skillsRect, allSkillsRect, chipButtonRect);
			}
		}


		private void DoSkillRecordList(Rect skillsRect, Rect allSkillsRect, Rect chipButtonRect)
		{
			try
			{
				Widgets.BeginScrollView(skillsRect, ref scrollPosition, allSkillsRect, false);
				float yPos = 0f;
				foreach (SkillRecord skillRecord in PreviewClone.skills.skills)
				{
					Rect skillLabelRect = new(skillsRect.xMin, skillsRect.yMin + yPos, chipButtonRect.xMax - chipButtonRect.xMin, SkillUI.SkillHeight);
					using (new TextBlock(TextAnchor.MiddleLeft))
					{
						Widgets.Label(skillLabelRect, skillRecord.def.LabelCap);
					}
					Rect skillLevelRect = new(skillLabelRect.xMax, skillLabelRect.y, SkillLevelRectWidth, skillLabelRect.height);
					if (skillRecord.TotallyDisabled)
					{
						using (new TextBlock(UIUtils.DisabledSkillColor))
						{
							Widgets.Label(skillLevelRect, "-");
						}
					}
					else
					{
						using (new TextBlock(TextAnchor.MiddleCenter))
						{
							TextBlock colorBlock;
							if (skillRecord.Level == 0 && skillRecord.Aptitude != 0)
							{
								colorBlock = new((skillRecord.Aptitude > 0) ? ColorLibrary.BrightGreen : ColorLibrary.RedReadable);
							}
							Widgets.Label(skillLevelRect, skillRecord.Level.ToStringCached());
						}

						Rect passionRect = new(skillsRect.xMax - UIUtils.PassionIconSize, skillLabelRect.y, UIUtils.PassionIconSize, UIUtils.PassionIconSize);
						if (skillRecord.passion != Passion.None)
						{
							Texture2D passionIcon = skillRecord.passion == Passion.Minor ? SkillUI.PassionMinorIcon : SkillUI.PassionMajorIcon;
							GUI.DrawTexture(passionRect, passionIcon);
						}
						if (Mouse.IsOver(passionRect))
						{
							Widgets.DrawHighlight(passionRect);
							string passionTooltip = CloneUtils.GetPassionModsTooltip(skillRecord, selectedSkillChips);
							if (passionTooltip.Length > 0)
							{
								TooltipHandler.TipRegion(passionRect, passionTooltip);
							}
						}


						Rect skillBarRect = new(skillLevelRect.xMax, skillLabelRect.y, passionRect.xMin - skillLevelRect.xMax - UIUtils.TinyPadding, skillLabelRect.height);
						float fillPercent = Math.Max(0.01f, skillRecord.Level / (float)SkillRecord.MaxLevel);
						Texture2D fillTex = TextureLibrary.SkillBarFillTex;
						if ((ModsConfig.BiotechActive || ModsConfig.AnomalyActive) && skillRecord.Aptitude != 0)
						{
							fillTex = (skillRecord.Aptitude > 0) ? TextureLibrary.SkillBarAptitudePositiveTex : TextureLibrary.SkillBarAptitudeNegativeTex;
						}
						Widgets.FillableBar(skillBarRect, fillPercent, fillTex, null, doBorder: false);
						if (Mouse.IsOver(skillBarRect))
						{
							Widgets.DrawHighlight(skillBarRect);
							string levelTooltip = CloneUtils.GetAptitudeModsTooltip(skillRecord, selectedSkillChips);
							if (levelTooltip.Length > 0)
							{
								TooltipHandler.TipRegion(skillBarRect, levelTooltip);
							}
						}
					}
					yPos += SkillUI.SkillHeight + UIUtils.TinyPadding;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}


		private void DoImprintFloatMenu()
		{
			List<FloatMenuOption> options = [];
			foreach (Thing thing in cloningVat.Map.listerThings.ThingsOfDef(SilverSquad_ThingDefOfs.SilverSquad_GenomeImprint))
			{
				if (!thing.PositionHeld.Fogged(cloningVat.Map))
				{
					ThingClass_GenomeImprint imprint = thing as ThingClass_GenomeImprint;
					if (imprint?.genome?.Clone is null) continue;
					options.Add(new(imprint.genome.Clone.Name.ToStringFull, () =>
					{
						this.imprint = imprint;
						backerPawn = Find.PawnDuplicator.Duplicate(imprint?.genome?.Clone);
						backerPawn.style.beardDef = imprint.genome.OriginalBeard;

						initialCloneCapacities.Clear();
						foreach (PawnCapacityDef cap in ProjectSilverSquad.AllHumanlikeCapacities)
						{
							if (!PawnCapacityUtility.BodyCanEverDoCapacity(PreviewClone.RaceProps.body, cap)) continue;

							initialCloneCapacities.Add(cap, PawnCapacityUtility.CalculateCapacityLevel(PreviewClone.health.hediffSet, cap).ToStringPercent());
						}

						originalHediffs.Clear();
						foreach (Hediff hediff in PreviewClone.health.hediffSet.hediffs)
						{
							if (hediff.Part is null) continue;
							originalHediffs.TryAdd(hediff.Part, []);
							originalHediffs[hediff.Part].Add(hediff);
						}
						preXenogermInfo = (PreviewClone.genes.Xenotype, PreviewClone.genes.xenotypeName, PreviewClone.genes.iconDef, [.. PreviewClone.genes.GenesListForReading]);
						selectedSkillChips.Clear();
						selectedSurgeries.Clear();
						selectedTraitChips.Clear();
					},
					PreviewClone, Color.white));
				}
			}
			if (options.Empty())
			{
				options.Add(new("SilverSquad_CloningVat_CharacterImprint_PortraitFloatMenuEmpty".Translate(), null));
			}
			Find.WindowStack.Add(new FloatMenu(options));
		}
	}
}
