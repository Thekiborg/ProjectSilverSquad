using LudeonTK;
using System.Linq;

namespace ProjectSilverSquad
{
	internal class Debug
	{
		[DebugAction("Cloning", "Generate genome info", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.Playing)]
		public static void GenerateGenomeInfo()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				if (item is Pawn pawn)
				{
					ThingClass_GenomeImprint GenomeImprint = (ThingClass_GenomeImprint)ThingMaker.MakeThing(SilverSquad_ThingDefOfs.SilverSquad_GenomeImprint);
					GenomeImprint.genome = new(pawn);
					GenSpawn.Spawn(GenomeImprint, pawn.Position, pawn.Map);
				}
			}
		}


		[DebugAction("Cloning", "Auto generate patches", allowedGameStates = AllowedGameStates.Entry)]
		public static void AutogenerateTraitPatches()
		{
			Find.WindowStack.Add(new Window_AutoGeneratePatches());
		}
	}
}
