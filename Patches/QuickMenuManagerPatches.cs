using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyDebug.Patches;

public class QuickMenuManagerPatches
{
	[HarmonyPatch(typeof(QuickMenuManager), "Start")]
	[HarmonyPrefix]
	static void StartPatch(QuickMenuManager __instance)
	{
		__instance.Debug_SetEnemyType(0);
		__instance.Debug_SetAllItemsDropdownOptions();
	}

	[HarmonyPatch(typeof(QuickMenuManager), "CanEnableDebugMenu")]
	[HarmonyPrefix]
	static bool DebugMenuPatch(ref bool __result)
	{
		__result = true;
		return false;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_SpawnEnemy")]
	[HarmonyPatch(typeof(QuickMenuManager), "Debug_SpawnItem")]
	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> RuntimeFixTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		return new CodeMatcher(instructions)
			.Start()
			.RemoveInstructions(10)
			.InstructionEnumeration();
	}
}
