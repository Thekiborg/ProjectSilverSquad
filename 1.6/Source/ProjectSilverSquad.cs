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
		internal static bool VREAndroidsActive = ModsConfig.IsActive("vanillaracesexpanded.android");
		internal static bool VREStarJacksActive = ModsConfig.IsActive("vanillaracesexpanded.starjack");

		public static readonly List<GeneDef> PotentiallyNonGameBreakingGenes = [.. DefDatabase<GeneDef>.AllDefsListForReading.Where(gene =>
			gene.GetType() == typeof(GeneDef) && // Want to get all genes that are base Rimworld Genedefs. Not subtypes
			(!VREAndroidsActive || !gene.defName.Contains("VREA_")) &&
			(!VREStarJacksActive || !gene.defName.Contains("_Astrogene"))
		)];
		public static readonly GameComponent_CloneSkillMods CloneSkillMods = Current.Game.GetComponent<GameComponent_CloneSkillMods>();
		public static readonly List<PawnCapacityDef> AllHumanlikeCapacities = [.. DefDatabase<PawnCapacityDef>.AllDefs.Where(x => x.showOnHumanlikes).OrderBy(capDef => capDef.listOrder)];

		static ProjectSilverSquad()
		{
			Harmony harmony = new("Thekiborg.ProjectSilverSquad");
			harmony.PatchAll();
		}
	}
}
