using System.Linq;
using System.Text;

namespace ProjectSilverSquad
{
	internal class Dialog_SelectBrainChipSkill : Window
	{
		private const float HeaderHeight = 35f;
		private const float skillModRectHeight = 22f;
		private const float ChipElementHeight = 32f;
		private static readonly Vector2 ButtonSize = new(150f, 38f);
		private float scrollViewHeight;
		private Vector2 scrollPosition;
		private readonly Window_CloningSettings settingsWindow;
		private readonly Dictionary<BrainChipDef, bool> selectedChips = [];
		private readonly Dictionary<BrainChipDef, List<Tuple<BrainChipSkillModification, Passion>>> prevPassions = [];
		private readonly List<BrainChipDef> chipsOnMap = [];


		public override Vector2 InitialSize => new(500f, 600f);


		public Dialog_SelectBrainChipSkill(Window_CloningSettings settingsWindow, Map map)
		{
			this.settingsWindow = settingsWindow;
			foreach (Thing chip in map.listerThings.AllThings.Where(t => t.def is BrainChipDef bcd && bcd.Category.HasFlag(BrainChipCategory.SkillOnly)))
			{
				if (!chip.PositionHeld.Fogged(map))
				{
					chipsOnMap.AddDistinct(chip.def as BrainChipDef);
				}
			}
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
				if (!selectedChips.TryGetValue(chip, out bool value))
				{
					selectedChips.Add(chip, true);
				}
				else
				{
					selectedChips[chip] = !value;
				}
				RegisterBrainChip(chip);
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
			foreach (var skillMod in chip.skillMods)
			{
				string skillReadout = $"{skillMod.skillDef.LabelCap} {skillMod.skillOffset.ToStringWithSign()}";
				float textWidth = Text.CalcSize(skillReadout).x;
				Rect skillModLabelRect = new(rect.xMin + UIUtils.TinyPadding,
											rect.yMax - (rect.height / 2f) - (skillModRectHeight / 2),
											textWidth,
											skillModRectHeight);

				Rect passionRect = new(0f, 0f, 0f, 0f);
				/*if (skillMod.passionMod != PassionMod.PassionModType.None)
				{
					Texture2D icon = skillMod.passionMod == PassionMod.PassionModType.AddOneLevel ? SkillUI.PassionMinorIcon : UIUtils.ExtinguishedPassion;
					passionRect = new(skillModLabelRect.xMax, skillModLabelRect.y, UIUtils.PassionIconSize, UIUtils.PassionIconSize);
					GUI.DrawTexture(passionRect, icon);
				}*/

				Rect hightlightRect = new(skillModLabelRect);
				hightlightRect.width += passionRect.width;

				if (skillModLabelRect.width + passionRect.width + UIUtils.TinyPadding > rect.width)
				{
					Rect doesntFitRect = new(rect.xMin + UIUtils.TinyPadding,
											skillModLabelRect.y,
											skillModRectHeight,
											skillModRectHeight);

					using (new TextBlock(TextAnchor.MiddleCenter))
					{
						Widgets.Label(doesntFitRect, "...");
					}

					if (Mouse.IsOver(doesntFitRect))
					{
						Widgets.DrawHighlight(doesntFitRect);

						StringBuilder sb = new();
						for (int i = chip.skillMods.IndexOf(skillMod); i < chip.skillMods.Count; i++)
						{
							sb.AppendLine(chip.skillMods[i].ToString());
							sb.AppendLine();
						}

						TooltipHandler.TipRegion(doesntFitRect, sb.ToString());
						break;
					}
				}
				else
				{
					Widgets.Label(skillModLabelRect, skillReadout);

					if (Mouse.IsOver(hightlightRect))
					{
						Widgets.DrawHighlight(hightlightRect);
						TooltipHandler.TipRegion(hightlightRect, skillMod.ToString());
					}
				}
				rect.xMin += skillModLabelRect.width + passionRect.width + UIUtils.TinyPadding;
			}
		}


		public override void Close(bool doCloseSound = true)
		{
			settingsWindow.selectedSkillChips = selectedChips;
			base.Close(doCloseSound);
		}


		private void RegisterBrainChip(BrainChipDef chip)
		{
			if (!ProjectSilverSquad.CloneSkillMods.AppliedSkillBrainChipsPerPawn.TryGetValue(settingsWindow.PreviewClone, out List<BrainChipDef> appliedBrainChips))
			{
				appliedBrainChips = [];
				ProjectSilverSquad.CloneSkillMods.AppliedSkillBrainChipsPerPawn.Add(settingsWindow.PreviewClone, appliedBrainChips);
			}

			if (selectedChips[chip])
			{
				appliedBrainChips.Add(chip);

				List<Tuple<BrainChipSkillModification, Passion>> prevPassions = [];
				foreach (var skillMod in chip.skillMods)
				{
					var record = settingsWindow.PreviewClone.skills.GetSkill(skillMod.skillDef);
					Passion passion = record.passion;
					/*if (skillMod.passionMod == PassionMod.PassionModType.AddOneLevel)
					{
						prevPassions.Add(new(skillMod, passion));
						passion = passion.AddTo(Passion.Minor);
					}*/
					record.passion = passion;
				}
				this.prevPassions.TryAdd(chip, prevPassions);
			}
			else
			{
				appliedBrainChips.Remove(chip);

				var prevPassions = this.prevPassions[chip];
				foreach (var skillModAndPassion in prevPassions)
				{
					var record = settingsWindow.PreviewClone.skills.GetSkill(skillModAndPassion.Item1.skillDef);
					Passion passion = record.passion;
					/*if (skillModAndPassion.Item1.passionMod == PassionMod.PassionModType.AddOneLevel)
					{
						passion = skillModAndPassion.Item2;
					}*/
					record.passion = passion;
				}

				this.prevPassions.Remove(chip);
			}

			settingsWindow.PreviewClone.skills.Notify_SkillDisablesChanged();
			settingsWindow.PreviewClone.skills.DirtyAptitudes();
		}
	}
}
