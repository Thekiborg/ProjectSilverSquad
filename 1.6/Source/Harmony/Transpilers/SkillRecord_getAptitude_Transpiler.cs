using System.Reflection.Emit;

namespace ProjectSilverSquad
{
	/// <summary>
	/// Adds brain chip calc and manually added trait skill calc to the aptitudes
	/// Aptitudes was the cleanest way to make this work after 2 days of rewrites
	/// Shouldn't have any performance impact on pawns that are not the fake one from the window
	/// </summary>
	[HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Aptitude), MethodType.Getter)]
	internal static class SkillRecord_getAptitude_Transpiler
	{
		private static void NewAptitudeCalcs(Pawn pawn, SkillDef def, ref int? aptitudeCached)
		{
			if (ProjectSilverSquad.CloneSkillMods.AppliedSkillBrainChipsPerPawn.TryGetValue(pawn, out List<BrainChipDef> skillBrainChips))
			{
				foreach (var brainChip in skillBrainChips)
				{
					foreach (var skillMod in brainChip.skillMods)
					{
						if (skillMod.skillDef != def) continue;

						aptitudeCached += skillMod.skillOffset;
					}
				}
			}
			if (ProjectSilverSquad.CloneSkillMods.AppliedBrainChipModsTrait.TryGetValue(pawn, out List<BrainChipDef> traitBrainChips))
			{
				foreach (var brainChip in traitBrainChips)
				{
					foreach (var traitMod in brainChip.traitMods)
					{
						Trait trait = pawn.story.traits.GetTrait(traitMod.traitDef);

						if (trait is null) continue;

						foreach (SkillGain skillGain in trait.CurrentData.skillGains)
						{
							if (skillGain.skill != def) continue;

							aptitudeCached += skillGain.amount;
						}
					}
				}
			}
		}


		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> AddNewAptitudeCalculationsForClones(IEnumerable<CodeInstruction> codeInstructions)
		{
			CodeMatcher codeMatcher = new(codeInstructions);

			var instructionsToMatch = new CodeMatch[]
			{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldflda),
				new(OpCodes.Call),
				new(OpCodes.Ret)
			};

			var instructionsToInsert = new CodeInstruction[]
			{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "pawn")),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), nameof(SkillRecord.def))),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldflda, AccessTools.Field(typeof(SkillRecord), "aptitudeCached")),
				new(OpCodes.Call, AccessTools.Method(typeof(SkillRecord_getAptitude_Transpiler), nameof(NewAptitudeCalcs))),
			};

			codeMatcher.End();
			codeMatcher.MatchStartBackwards(instructionsToMatch);
			codeMatcher.Advance(-1);

			if (codeMatcher.IsInvalid)
			{
				Log.Error("ProjectSilverSquad.Harmony.SkillRecord_getAptitude_Transpiler couldn't patch it's intended method");
				return codeInstructions;
			}
			else
			{
				codeMatcher.Insert(instructionsToInsert);
				return codeMatcher.InstructionEnumeration();
			}
		}
	}
}
