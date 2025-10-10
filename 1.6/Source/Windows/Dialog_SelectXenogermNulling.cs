using System.Linq;

namespace ProjectSilverSquad
{
	/// <summary>
	/// Copy of Rimworld.Dialog_SelectXenogerm with a modified Accept() method, and having all Close() calls call Accept() instead
	/// of course everything was private so i had to copy it all
	/// </summary>
	public class Dialog_SelectXenogermNulling : Window
	{
		private static readonly Vector2 ButSize = new(150f, 38f);
		private readonly Dictionary<string, string> truncateCache = [];

		private readonly Pawn pawn;
		private readonly List<Xenogerm> xenogerms = [];
		private Xenogerm selected;
		private Vector2 scrollPosition;
		private float scrollViewHeight;
		private readonly Action<Xenogerm> onSelect;

		public override Vector2 InitialSize => new(500f, 600f);

		public Dialog_SelectXenogermNulling(Pawn pawn, Map map, Xenogerm initialSelected, Action<Xenogerm> onSelect)
		{
			this.pawn = pawn;
			this.onSelect = onSelect;
			foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.Xenogerm))
			{
				if (!item.PositionHeld.Fogged(map))
				{
					xenogerms.Add((Xenogerm)item);
				}
			}
			if (initialSelected != null && xenogerms.Contains(initialSelected))
			{
				selected = initialSelected;
			}
			closeOnAccept = false;
			absorbInputAroundWindow = true;
		}

		public override void PostOpen()
		{
			if (!ModLister.CheckBiotech("xenogerm"))
			{
				Close(doCloseSound: false);
			}
			else
			{
				base.PostOpen();
			}
		}

		public override void DoWindowContents(Rect rect)
		{
			Rect rect2 = rect;
			rect2.yMax -= ButSize.y + 4f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect2, "SelectXenogerm".Translate());
			Text.Font = GameFont.Small;
			rect2.yMin += 39f;
			DisplayXenogerms(rect2);
			Rect rect3 = rect;
			rect3.yMin = rect3.yMax - ButSize.y;
			if (selected != null)
			{
				if (Widgets.ButtonText(new Rect(rect3.xMax - ButSize.x, rect3.y, ButSize.x, ButSize.y), "Accept".Translate()))
				{
					Accept();
				}
				if (Widgets.ButtonText(new Rect(rect3.x, rect3.y, ButSize.x, ButSize.y), "Close".Translate()))
				{
					Accept();
				}
			}
			else if (Widgets.ButtonText(new Rect((rect3.width - ButSize.x) / 2f, rect3.y, ButSize.x, ButSize.y), "Close".Translate()))
			{
				Accept();
			}
		}

		private void DisplayXenogerms(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(4f);
			GUI.BeginGroup(rect);
			Rect viewRect = new(0f, 0f, rect.width - 16f, scrollViewHeight);
			float num = 0f;
			Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);
			for (int i = 0; i < xenogerms.Count; i++)
			{
				float num2 = rect.width;
				if (scrollViewHeight > rect.height)
				{
					num2 -= 16f;
				}
				DrawXenogerm(new Rect(0f, num, num2, 32f), i);
				num += 32f;
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		private void DrawXenogerm(Rect rect, int index)
		{
			Xenogerm xenogerm = xenogerms[index];
			if (index % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect);
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (selected == xenogerm)
			{
				Widgets.DrawHighlightSelected(rect);
			}
			Widgets.InfoCardButton(rect.xMax - 24f, rect.y + 4f, xenogerm);
			rect.xMax -= 36f;
			for (int num = Mathf.Min(xenogerm.GeneSet.GenesListForReading.Count, 10) - 1; num >= 0; num--)
			{
				GeneDef geneDef = xenogerm.GeneSet.GenesListForReading[num];
				Rect rect2 = new(rect.xMax - 11f, rect.yMax - (rect.height / 2f) - 11f, 22f, 22f);
				Widgets.DefIcon(rect2, geneDef, null, 1.25f);
				Rect rect3 = rect2;
				rect3.yMin = rect.yMin;
				rect3.yMax = rect.yMax;
				if (Mouse.IsOver(rect3))
				{
					Widgets.DrawHighlight(rect3);
					TooltipHandler.TipRegion(rect3, geneDef.LabelCap + "\n\n" + geneDef.DescriptionFull);
				}
				rect.xMax -= 22f;
			}
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, () => xenogerm.LabelCap + "\n\n" + "Genes".Translate().CapitalizeFirst() + ":\n" + xenogerm.GeneSet.GenesListForReading.Select(x => x.LabelCap.ToString()).ToLineList("  - "), 128921381);
			}
			rect.xMin += 4f;

			string xenogermName = xenogerm.LabelCap.Truncate(rect.width, truncateCache);
			using (new TextBlock(TextAnchor.MiddleLeft))
			{
				Widgets.Label(rect, xenogermName);
			}

			string metabLabel = xenogerm.GeneSet.MetabolismTotal.ToStringWithSign();
			float metabWidth = Text.CalcSize(metabLabel).x;
			Rect metabRect = new(rect.xMin + Text.CalcSize(xenogermName).x + UIUtils.TinyPadding,
				rect.y,
				metabWidth + UIUtils.PassionIconSize + UIUtils.TinyPadding,
				rect.height);

			float iconSize = 18f;
			Rect metabIconRect = new(metabRect.xMin,
				metabRect.yMin + (metabRect.height / 2) - (iconSize / 2),
				iconSize,
				iconSize);
			Rect metabLabelRect = new(metabIconRect.xMax + UIUtils.TinyPadding, metabRect.y, metabWidth, metabRect.height);
			GUI.DrawTexture(metabIconRect, GeneUtility.METTex.Texture);
			using (new TextBlock(TextAnchor.MiddleCenter))
			{
				Widgets.Label(metabLabelRect, metabLabel);
			}

			if (Widgets.ButtonInvisible(rect))
			{
				selected = xenogerm;
			}
		}


		private void Accept()
		{
			if (pawn is not null && selected is not null)
			{
				int num = GeneUtility.MetabolismAfterImplanting(pawn, selected.GeneSet);
				if (num < GeneTuning.BiostatRange.TrueMin)
				{
					Messages.Message(string.Concat("OrderImplantationIntoPawn".Translate(pawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "ResultingMetTooLow".Translate() + " (", num.ToString(), ")"), pawn, MessageTypeDefOf.RejectInput, historical: false);
					return;
				}
				if (selected.PawnIdeoDisallowsImplanting(pawn))
				{
					Messages.Message("CannotGenericWorkCustom".Translate("OrderImplantationIntoPawn".Translate(pawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "IdeoligionForbids".Translate()), pawn, MessageTypeDefOf.RejectInput, historical: false);
					return;
				}
			}
			onSelect?.Invoke(selected);
			Close();
		}
	}
}
