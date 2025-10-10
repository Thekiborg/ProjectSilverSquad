using System.Linq;
using System.Text;

namespace ProjectSilverSquad
{
	internal class Window_AutoGeneratePatches : Window
	{
		private const float ButtonTitleHeight = 60f;
		private const float ElementPadding = 10f;
		private const float ButtonHeight = 40f;
		private const float ButtonWidth = 100f;
		private ModContentPack pickedMod;
		private Vector2 scrollPos;
		private readonly Dictionary<TraitDef, bool> checkboxDict = [];

		public override Vector2 InitialSize => new(400f, 650f);
		public override void DoWindowContents(Rect inRect)
		{
			Rect titleRect = new(inRect.xMin, inRect.yMin, inRect.width, ButtonTitleHeight);
			using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter))
			{
				Widgets.Label(titleRect, pickedMod?.Name ?? "Click here");
				if (Widgets.ButtonInvisible(titleRect))
				{
					List<FloatMenuOption> options = [];
					foreach (ModContentPack mod in LoadedModManager.RunningMods)
					{
						options.Add(new(mod.Name, () =>
						{
							pickedMod = mod;
							checkboxDict.Clear();
						}));
					}
					Find.WindowStack.Add(new FloatMenu(options));
				}
			}
			Rect buttonRect = new((inRect.width / 2) - (ButtonWidth / 2), inRect.height - ButtonHeight, ButtonWidth, ButtonHeight);
			if (Widgets.ButtonText(buttonRect, "Export"))
			{
				StringBuilder sb = new();
				string packageid = pickedMod.PackageId;
				sb.AppendLine("OUTPUT - AS ERROR SO THE CONSOLE OPENS AUTOMATICALLY");
				foreach (KeyValuePair<TraitDef, bool> kvp in checkboxDict)
				{
					if (pickedMod.IsCoreMod)
					{
						sb.AppendLine($"<{kvp.Key.defName}>{kvp.Value}</{kvp.Key.defName}>");
					}
					else
					{
						sb.AppendLine($"<{kvp.Key.defName} MayRequire=\"{packageid}\">{kvp.Value}</{kvp.Key.defName}>");
					}
				}
				Log.Error(sb.ToString());
				Close();
			}

			if (pickedMod is not null)
			{
				Rect viewRect = new(inRect.x, titleRect.yMax + ElementPadding, inRect.width, inRect.height - ElementPadding - buttonRect.height - titleRect.height - ElementPadding);
				List<TraitDef> allTraitDefsFromMod = [.. pickedMod?.AllDefs.Where(def => def.GetType() == typeof(TraitDef)).Select(def => def as TraitDef)];
				int traitCount = allTraitDefsFromMod.Count;

				Rect allTraitsRect = new(viewRect.x, viewRect.yMin, viewRect.width - GenUI.ScrollBarWidth, Widgets.CheckboxSize * traitCount);
				float yPos = 0f;

				try
				{
					Widgets.BeginScrollView(viewRect, ref scrollPos, allTraitsRect);
					foreach (TraitDef def in allTraitDefsFromMod)
					{
						checkboxDict.TryAdd(def, false);
						bool checkboxBool = checkboxDict[def];
						Rect traitRect = new(allTraitsRect.x, allTraitsRect.yMin + yPos, allTraitsRect.width, Widgets.CheckboxSize);
						Widgets.CheckboxLabeled(traitRect, def.defName, ref checkboxBool);
						checkboxDict[def] = checkboxBool;
						yPos += traitRect.height;
					}
				}
				finally
				{
					Widgets.EndScrollView();
				}
			}
		}
	}
}
