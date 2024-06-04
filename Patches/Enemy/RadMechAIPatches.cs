using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(RadMechAI))]
public class RadMechAIPatches
{
	[HarmonyPatch("CheckSightForThreat")]
	[HarmonyPostfix]
	static void CheckSightForThreatPostfixPatch(RadMechAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return;

		Draw.Sphere(__instance.eye.position + __instance.eye.forward * 58f + -__instance.eye.up * 10f, 60f, duration: __instance.AIIntervalTime, color: new Color(1f, 0f, 0f, 0.2f));
	}
}
