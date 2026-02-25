using System.Reflection.Emit;

namespace ProjectSilverSquad
{
	/// <summary>
	/// Skips a bunch of NREs if the given pawn is dead<br></br>
	/// Why should I be the one doing this and not ludeon?<br></br>
	/// Why are they deleting bug reports instead of fixing them?<br></br>
	/// Fuck if i know
	/// </summary>
	[HarmonyPatch(typeof(GameComponent_PawnDuplicator), nameof(GameComponent_PawnDuplicator.Duplicate))]
	internal static class GameComponent_PawnDuplicator_Duplicate_Transpiler
	{
		/// <summary>
		/// <code>
		/// if (ModsConfig.AnomalyActive)
		/// {
		///		int duplicateOf = ((pawn.duplicate.duplicateOf == int.MinValue) ? pawn.thingIDNumber : pawn.duplicate.duplicateOf);
		///		pawn.duplicate.duplicateOf = duplicateOf;
		///		pawn2.duplicate.duplicateOf = duplicateOf;
		/// }
		/// </code>
		/// </summary>
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> SkipDuplicateCheckIfDead(IEnumerable<CodeInstruction> codeInstructions)
		{
			CodeMatcher codeMatcher = new(codeInstructions);

			var instructionsToMatch = new CodeMatch[]
			{
				new(OpCodes.Stloc_2),
				new(OpCodes.Call),
				new(OpCodes.Brfalse_S),
			};

			var instructionsToInsert = new CodeInstruction[]
			{
				new(OpCodes.Ldarg_1),
				new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Dead))),
				new(OpCodes.Brtrue)
			};

			codeMatcher.MatchEndForward(instructionsToMatch);

			var jump = codeMatcher.Instruction.operand;
			instructionsToInsert[2].operand = jump;

			codeMatcher.Advance(1);

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


		/// <summary>
		/// <code>
		/// CopyNeeds(pawn, pawn2);
		/// </code>
		/// </summary>
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> SkipNeedsIfDead(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
		{
			CodeMatcher codeMatcher = new(codeInstructions, ilg);

			var instructionsToMatch = new CodeMatch[]
			{
				new(OpCodes.Ldarg_1),
				new(OpCodes.Ldloc_2),
				new(OpCodes.Call, AccessTools.Method(typeof(GameComponent_PawnDuplicator), "CopyNeeds")),
			};

			var instructionsToInsert = new CodeInstruction[]
			{
				new(OpCodes.Ldarg_1),
				new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Dead))),
				new(OpCodes.Brtrue)
			};

			codeMatcher.End();
			codeMatcher.MatchEndBackwards(instructionsToMatch);
			codeMatcher.Advance(1);

			codeMatcher.CreateLabel(out var label);
			instructionsToInsert[2].operand = label;

			codeMatcher.MatchStartBackwards(instructionsToMatch);

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
