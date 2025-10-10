global using HarmonyLib;
global using RimWorld;
global using System;
global using System.Collections.Generic;
global using UnityEngine;
global using Verse;
using System.Linq;
//global using Verse.AI;

namespace ProjectSilverSquad
{
	public static class ProjectSilverSquad
	{
		public static readonly GameComponent_CloneSkillMods CloneSkillMods = Current.Game.GetComponent<GameComponent_CloneSkillMods>();
		public static readonly List<PawnCapacityDef> AllHumanlikeCapacities = [.. DefDatabase<PawnCapacityDef>.AllDefs.Where(x => x.showOnHumanlikes).OrderBy(capDef => capDef.listOrder)];

		static ProjectSilverSquad()
		{
			Harmony harmony = new("Thekiborg.ProjectSilverSquad");
			harmony.PatchAll();
		}
	}
}
