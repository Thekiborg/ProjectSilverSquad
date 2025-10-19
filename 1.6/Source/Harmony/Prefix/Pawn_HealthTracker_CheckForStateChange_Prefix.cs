namespace ProjectSilverSquad
{
	[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.CheckForStateChange))]
	internal static class Pawn_HealthTracker_CheckForStateChange_Prefix
	{
		//internal static bool PreventPawnDeathOnCloningWindow(Pawn __instance)
		//{
		//	if (__instance.ParentHolder)
		//}
	}
}
