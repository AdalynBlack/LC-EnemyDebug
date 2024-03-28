using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

[HarmonyPatch(typeof(SpringManAI))]
public class SpringManPatches
{
	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(SpringManAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return;

		if(__instance.currentBehaviourStateIndex == 1)
		{
			GizmoPatches.DrawSphere(__instance.transform.position + Vector3.up * 1.6f, 0.2f);
		} else {
			GizmoPatches.DrawSphere(__instance.transform.position + Vector3.up * 0.5f, 0.2f);
		}
	}
}
