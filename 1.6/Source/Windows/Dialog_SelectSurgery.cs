using System.Linq;

namespace ProjectSilverSquad
{
	internal class Dialog_SelectSurgery : Window
	{
		private const float HeaderHeight = 35f;
		private const float SurgeryElementHeight = 32f;
		private static readonly Vector2 ButtonSize = new(150f, 38f);

		private readonly Window_CloningSettings settingsWindow;
		private readonly Dictionary<(RecipeDef, BodyPartRecord), bool> selectedSurgeries = [];
		private readonly List<(RecipeDef, BodyPartRecord)> recipes = [];
		private readonly Type[] allowedRecipeTypes =
			[
				typeof(Recipe_InstallArtificialBodyPart),
				typeof(Recipe_InstallImplant),
				typeof(Recipe_InstallNaturalBodyPart),
				typeof(Recipe_ImplantIUD),
			];
		private readonly Map map;

		private float scrollViewHeight;
		private Vector2 scrollPosition;
		public Xenogerm xenogerm;

		public override Vector2 InitialSize => new(500f, 600f);


		public Dialog_SelectSurgery(Window_CloningSettings settingsWindow, Map map)
		{
			this.settingsWindow = settingsWindow;
			this.map = map;
		}


		public override void DoWindowContents(Rect rect)
		{
			Rect titleRect = rect;
			titleRect.yMax -= ButtonSize.y + GenUI.GapTiny;
			using (new TextBlock(GameFont.Medium))
			{
				Widgets.Label(titleRect, "AddBill".Translate());
			}
			titleRect.yMin += HeaderHeight + GenUI.GapTiny;
			ListSurgeries(titleRect);
			Rect buttonsRect = rect;
			buttonsRect.yMin = buttonsRect.yMax - ButtonSize.y;
			if (Widgets.ButtonText(new Rect((buttonsRect.width - ButtonSize.x) / 2f, buttonsRect.y, ButtonSize.x, ButtonSize.y), "Close".Translate()))
			{
				Close();
			}
		}


		private void ListSurgeries(Rect rect)
		{
			GetAvailableSurgeriesOnPawn(map);
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(4f);
			GUI.BeginGroup(rect);
			Rect viewRect = new(0f, 0f, rect.width - GenUI.ScrollBarWidth, scrollViewHeight);
			float yPos = 0f;
			Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
			for (int i = 0; i < recipes.Count; i++)
			{
				float adjustedWidth = rect.width;
				if (scrollViewHeight > rect.height)
				{
					adjustedWidth -= GenUI.ScrollBarWidth;
				}
				DoSurgRow(new Rect(0f, yPos, adjustedWidth, SurgeryElementHeight), i);
				yPos += SurgeryElementHeight;
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = yPos;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}


		private void DoSurgRow(Rect rect, int index)
		{
			var surgery = recipes[index];
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
				if (surgery.Item1 == RecipeDefOf.ImplantXenogerm)
				{
					Find.WindowStack.Add(new Dialog_SelectXenogermNulling(settingsWindow.PreviewClone, map, null, xeno =>
					{
						xenogerm = xeno;
						if (xenogerm is not null)
						{
							selectedSurgeries.TryAdd(surgery, true);
							selectedSurgeries[surgery] = true;
						}
						else
						{
							selectedSurgeries.TryAdd(surgery, false);
							selectedSurgeries[surgery] = false;
						}
						RegisterXenogermResult(surgery);
					}));
				}
				else
				{
					if (!selectedSurgeries.TryGetValue(surgery, out bool value))
					{
						selectedSurgeries.Add(surgery, true);
					}
					else
					{
						selectedSurgeries[surgery] = !value;
					}
					RegisterSurgeryResult(surgery);
				}
			}
			if (selectedSurgeries.TryGetValue(surgery, out var selected) && selected)
			{
				Widgets.DrawHighlightSelected(rect);
			}
			using (new TextBlock(TextAnchor.MiddleLeft))
			{
				Rect traitModLabelRect = new(rect.xMin + UIUtils.TinyPadding,
							rect.y,
							rect.width,
							rect.height);

				string label = surgery.Item1.Worker.GetLabelWhenUsedOn(settingsWindow.PreviewClone, surgery.Item2).CapitalizeFirst();
				if (surgery.Item2 != null && !surgery.Item1.hideBodyPartNames)
				{
					label = label + " (" + surgery.Item2.Label + ")";
				}
				Widgets.Label(traitModLabelRect, label);

				Rect hightlightRect = new(rect);
				if (Mouse.IsOver(hightlightRect))
				{
					Widgets.DrawHighlight(hightlightRect);
					TooltipHandler.TipRegion(hightlightRect, surgery.Item1.description);
				}
			}
		}


		public override void Close(bool doCloseSound = true)
		{
			settingsWindow.selectedSurgeries = selectedSurgeries;
			base.Close(doCloseSound);
		}


		private void RegisterSurgeryResult((RecipeDef, BodyPartRecord) surgeryWithPart)
		{
			if (selectedSurgeries[surgeryWithPart])
			{
				settingsWindow.PreviewClone.health.AddHediff(surgeryWithPart.Item1.addsHediff, surgeryWithPart.Item2);
				if (!surgeryWithPart.Item2.parts.NullOrEmpty())
				{
					foreach (var part in surgeryWithPart.Item2.GetPartAndAllChildParts())
					{
						// disables other any previous recipes on the same part
						foreach (var surg in selectedSurgeries)
						{
							if (surg.Key.Item2 == part && surg.Key.Item1 != surgeryWithPart.Item1)
							{
								selectedSurgeries[surg.Key] = false;
								RegisterSurgeryResult(surg.Key);
								break;
							}
						}
					}
				}
			}
			else
			{
				settingsWindow.PreviewClone.health.hediffSet.TryGetHediff(surgeryWithPart.Item1.addsHediff, out Hediff foundHediffOnPart);
				if (foundHediffOnPart is not null)
				{
					settingsWindow.PreviewClone.health.RemoveHediff(foundHediffOnPart);
					settingsWindow.PreviewClone.health.RestorePart(surgeryWithPart.Item2);
					RestoreOriginalHediffRecursively(surgeryWithPart.Item2);
				}
			}
		}


		private void RestoreOriginalHediffRecursively(BodyPartRecord part)
		{
			Log.Message(part);
			settingsWindow.originalHediffs.TryGetValue(part, out Hediff hediff);
			if (hediff is not null)
			{
				settingsWindow.PreviewClone.health.AddHediff(hediff, part);
			}
			for (int i = 0; i < part.parts.Count; i++)
			{
				RestoreOriginalHediffRecursively(part.parts[i]);
			}
		}


		private void RegisterXenogermResult((RecipeDef, BodyPartRecord) surgeryWithPart)
		{
			if (selectedSurgeries[surgeryWithPart])
			{
				if (xenogerm is null) return;
				settingsWindow.PreviewClone.genes.SetXenotype(XenotypeDefOf.Baseliner);
				settingsWindow.PreviewClone.genes.xenotypeName = xenogerm.xenotypeName;
				settingsWindow.PreviewClone.genes.iconDef = xenogerm.iconDef;
				foreach (GeneDef item in xenogerm.GeneSet.GenesListForReading)
				{
					settingsWindow.PreviewClone.genes.AddGene(item, xenogene: true);
				}
			}
			else
			{
				var info = settingsWindow.preXenogermInfo;
				settingsWindow.PreviewClone.genes.SetXenotype(info.Item1);
				settingsWindow.PreviewClone.genes.xenotypeName = info.Item2;
				settingsWindow.PreviewClone.genes.iconDef = info.Item3;
				foreach (Gene gene in info.Item4)
				{
					settingsWindow.PreviewClone.genes.AddGene(gene.def, xenogene: true);
				}
			}
		}


		private void GetAvailableSurgeriesOnPawn(Map map)
		{
			recipes.Clear();
			recipes.Add((RecipeDefOf.ImplantXenogerm, null));
			foreach (var surg in selectedSurgeries)
			{
				if (surg.Value)
					recipes.AddDistinct(surg.Key);
			}
			foreach (RecipeDef recipe in settingsWindow.PreviewClone.def.AllRecipes)
			{
				if (allowedRecipeTypes.Contains(recipe.workerClass) && recipe.AvailableNow)
				{
					AcceptanceReport report = recipe.Worker.AvailableReport(settingsWindow.PreviewClone);
					if (report.Accepted || !report.Reason.NullOrEmpty())
					{
						IEnumerable<ThingDef> enumerable = recipe.PotentiallyMissingIngredients(null, map);
						if (!enumerable.Any(x => x.isTechHediff) && !enumerable.Any(x => x.IsDrug) && (!enumerable.Any() || !recipe.dontShowIfAnyIngredientMissing))
						{
							if (recipe.targetsBodyPart)
							{
								foreach (BodyPartRecord bodyPart in recipe.Worker.GetPartsToApplyOn(settingsWindow.PreviewClone, recipe))
								{
									if (recipe.AvailableOnNow(settingsWindow.PreviewClone, bodyPart))
									{
										recipes.AddDistinct((recipe, bodyPart));
									}
								}
							}
							else if (!settingsWindow.PreviewClone.health.hediffSet.HasHediff(recipe.addsHediff))
							{
								recipes.AddDistinct((recipe, null));
							}
						}
					}
				}
			}
		}
	}
}
