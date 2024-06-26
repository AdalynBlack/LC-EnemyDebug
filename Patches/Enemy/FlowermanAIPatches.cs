using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(FlowermanAI))]
public class FlowermanAIPatches
{
	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(FlowermanAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return;

		if(__instance.favoriteSpot == null)
			return;

		Draw.Sphere(__instance.favoriteSpot.position, 1f, color: new Color(0f, 0f, 1f));
	}
}
