using System.Linq;
using System.Text;

namespace ProjectSilverSquad
{
	internal class Dialog_SelectBrainChipTrait : Window
	{
		private const float HeaderHeight = 35f;
		private const float traitModRectHeight = 22f;
		private const float ChipElementHeight = 32f;
		private static readonly Vector2 ButtonSize = new(150f, 38f);
		private float scrollViewHeight;
		private Vector2 scrollPosition;
		private readonly Window_CloningSettings settingsWindow;
		private readonly Dictionary<ThingClass_BrainChip, bool> selectedChips = [];
		private readonly List<ThingClass_BrainChip> chipsOnMap = [];


		public override Vector2 InitialSize => new(500f, 600f);


		public Dialog_SelectBrainChipTrait(Window_CloningSettings settingsWindow, Map map)
		{
			this.settingsWindow = settingsWindow;
			foreach (Thing chip in map.listerThings.AllThings.Where(t =>
				t is ThingClass_BrainChip tChip
				&& t.def is BrainChipDef
				&& CloneUtils.CategoryFor(tChip).HasFlag(BrainChipCategory.TraitOnly)))
			{
				if (!chip.PositionHeld.Fogged(map))
				{
					chipsOnMap.Add(chip as ThingClass_BrainChip);
				}
			}
			selectedChips = settingsWindow.selectedTraitChips;
		}


		public override void DoWindowContents(Rect rect)
		{
			Rect titleRect = rect;
			titleRect.yMax -= ButtonSize.y + GenUI.GapTiny;
			using (new TextBlock(GameFont.Medium))
			{
				Widgets.Label(titleRect, "SilverSquad_CloningVat_SelectBrainChip".Translate());
			}
			titleRect.yMin += HeaderHeight + GenUI.GapTiny;
			ListBrainChips(titleRect);
			Rect buttonsRect = rect;
			buttonsRect.yMin = buttonsRect.yMax - ButtonSize.y;
			if (Widgets.ButtonText(new Rect((buttonsRect.width - ButtonSize.x) / 2f, buttonsRect.y, ButtonSize.x, ButtonSize.y), "Close".Translate()))
			{
				Close();
			}
		}


		private void ListBrainChips(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(4f);
			GUI.BeginGroup(rect);
			Rect viewRect = new(0f, 0f, rect.width - GenUI.ScrollBarWidth, scrollViewHeight);
			float yPos = 0f;
			Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
			for (int i = 0; i < chipsOnMap.Count; i++)
			{
				float adjustedWidth = rect.width;
				if (scrollViewHeight > rect.height)
				{
					adjustedWidth -= GenUI.ScrollBarWidth;
				}
				DoChipRow(new Rect(0f, yPos, adjustedWidth, ChipElementHeight), i);
				yPos += ChipElementHeight;
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = yPos;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}


		private void DoChipRow(Rect rect, int index)
		{
			var chip = chipsOnMap[index];
			if (index % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect);
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (Widgets.ButtonInvisible(rect))
			{
				// Select the skill chip from this dialog if it's hybrid
				if (CloneUtils.CategoryFor(chip) == BrainChipCategory.Hybrid)
				{
					if (!settingsWindow.selectedSkillChips.TryGetValue(chip, out bool val))
					{
						settingsWindow.selectedSkillChips.Add(chip, true);
					}
					else
					{
						settingsWindow.selectedSkillChips[chip] = !val;
					}
					settingsWindow.Dialog_SelectBrainChipSkill.RegisterBrainChip(chip);
				}

				if (!selectedChips.TryGetValue(chip, out bool value))
				{
					selectedChips.Add(chip, true);
				}
				else
				{
					selectedChips[chip] = !value;
				}

				RegisterLateAddedTraitWithSkills(chip);
			}
			if (selectedChips.TryGetValue(chip, out var selected) && selected)
			{
				Widgets.DrawHighlightSelected(rect);
			}
			using (new TextBlock(TextAnchor.MiddleLeft))
			{
				float textWidth = Text.CalcSize(chip.LabelCap).x;
				Widgets.Label(rect, $"{chip.LabelCap}:");
				rect.xMin += textWidth + UIUtils.TinyPadding;
			}
			foreach (var traitMod in chip.data.traitMods)
			{
				string traitReadout = $"{traitMod.traitDef.DataAtDegree(traitMod.traitDegree).LabelCap}";
				float textWidth = Text.CalcSize(traitReadout).x;
				Rect traitModLabelRect = new(rect.xMin + UIUtils.TinyPadding,
											rect.yMax - (rect.height / 2f) - (traitModRectHeight / 2),
											textWidth,
											traitModRectHeight);

				Rect hightlightRect = new(traitModLabelRect);

				if (traitModLabelRect.width + UIUtils.TinyPadding > rect.width)
				{
					Rect doesntFitRect = new(rect.xMin + UIUtils.TinyPadding,
											traitModLabelRect.y,
											traitModRectHeight,
											traitModRectHeight);

					using (new TextBlock(TextAnchor.MiddleCenter))
					{
						Widgets.Label(doesntFitRect, "...");
					}

					if (Mouse.IsOver(doesntFitRect))
					{
						Widgets.DrawHighlight(doesntFitRect);

						StringBuilder sb = new();
						for (int i = chip.data.traitMods.IndexOf(traitMod); i < chip.data.traitMods.Count; i++)
						{
							sb.AppendLine(chip.data.traitMods[i].ToString());
							sb.AppendLine();
						}

						TooltipHandler.TipRegion(doesntFitRect, sb.ToString());
						break;
					}
				}
				else
				{
					Widgets.Label(traitModLabelRect, traitReadout);

					if (Mouse.IsOver(hightlightRect))
					{
						Widgets.DrawHighlight(hightlightRect);
						TooltipHandler.TipRegion(hightlightRect,
							traitMod.traitDef.DataAtDegree(traitMod.traitDegree).description
							.Formatted(settingsWindow.PreviewClone.Named("PAWN"))
							.AdjustedFor(settingsWindow.PreviewClone)
							.Resolve());
					}
				}
				rect.xMin += traitModLabelRect.width + UIUtils.TinyPadding;
			}
		}


		internal void RegisterLateAddedTraitWithSkills(ThingClass_BrainChip chip)
		{
			if (!ProjectSilverSquad.CloneSkillMods.AppliedTraitBrainChipsPerPawn.TryGetValue(settingsWindow.PreviewClone, out List<ThingClass_BrainChip> appliedBrainChips))
			{
				appliedBrainChips = [];
				ProjectSilverSquad.CloneSkillMods.AppliedTraitBrainChipsPerPawn.Add(settingsWindow.PreviewClone, appliedBrainChips);
			}

			if (selectedChips[chip])
			{
				appliedBrainChips.Add(chip);
				foreach (var traitMod in chip.data.traitMods)
				{
					Trait trait = new(traitMod.traitDef, traitMod.traitDegree);
					settingsWindow.PreviewClone.story.traits.GainTrait(trait);
				}
			}
			else
			{
				appliedBrainChips.Remove(chip);

				foreach (var traitMod in chip.data.traitMods)
				{
					Trait trait = settingsWindow.PreviewClone.story.traits.GetTrait(traitMod.traitDef);
					if (trait != null)
						settingsWindow.PreviewClone.story.traits.RemoveTrait(trait);
				}
			}

			settingsWindow.PreviewClone.skills.Notify_SkillDisablesChanged();
			settingsWindow.PreviewClone.skills.DirtyAptitudes();
		}
	}
}
