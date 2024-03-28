using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

[HarmonyPatch(typeof(HoarderBugAI))]
public class HoarderBugPatches
{
	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(HoarderBugAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return;

		if(__instance.nestPosition == null)
			return;

		GizmoPatches.DrawSphere(__instance.nestPosition, 0.8f, color: new Color(1f, 0.5f, 0f));
	}
}
