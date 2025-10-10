namespace ProjectSilverSquad
{
	public static class UIUtils
	{
		public const float TinyPadding = 5f;
		public const float SmallPadding = 10f;
		public const float MediumPadding = 15f;
		public const float PassionIconSize = 24f;
		public const float TraitTileHeight = 22f;
		public static readonly Color DisabledSkillColor = new(1f, 1f, 1f, 0.5f);


		public static void DoTraitMosaic(Rect boundsRect, Pawn clone, Dictionary<BrainChipDef, bool> allChips)
		{
			List<Trait> traits = clone.story.traits.TraitsSorted;
			float traitRectX = boundsRect.xMin;
			float traitRectY = boundsRect.yMin;
			foreach (Trait trait in traits)
			{
				float traitRectWidth = Text.CalcSize(trait.LabelCap).x + (TinyPadding * 2);
				if (traitRectX + traitRectWidth > boundsRect.xMax)
				{
					traitRectY += TraitTileHeight + TinyPadding;
					traitRectX = boundsRect.xMin;
				}
				Rect traitRect = new(traitRectX, traitRectY, traitRectWidth, TraitTileHeight);
				using (new TextBlock(CharacterCardUtility.StackElementBackground))
				{
					GUI.DrawTexture(traitRect, BaseContent.WhiteTex);
				}
				if (Mouse.IsOver(traitRect))
				{
					Widgets.DrawHighlight(traitRect);
				}
				{
					TextBlock tx;
					if (trait.Suppressed)
					{
						tx = new(ColoredText.SubtleGrayColor);
					}
					else if (trait.sourceGene != null)
					{
						tx = new(ColoredText.GeneColor);
					}
					else if (AddedByChip(trait.def, allChips))
					{
						tx = new(ColorLibrary.Green);
					}
					using (new TextBlock(TextAnchor.MiddleCenter))
					{
						Widgets.Label(traitRect, trait.LabelCap);
					}
					if (Mouse.IsOver(traitRect))
					{
						TooltipHandler.TipRegion(traitRect, new TipSignal(() => trait.TipString(clone), (int)traitRectY * 37));
					}
				}

				traitRectX += traitRect.width + TinyPadding;
			}
		}


		private static bool AddedByChip(TraitDef trait, Dictionary<BrainChipDef, bool> allChips)
		{
			if (allChips is null) return false;

			foreach (var kvp in allChips)
			{
				if (!kvp.Value) continue;

				foreach (var traitMods in kvp.Key.traitMods)
				{
					if (trait == traitMods.traitDef)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
