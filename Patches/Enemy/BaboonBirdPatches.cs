using EnemyDebug.Config;
using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(BaboonBirdAI))]
public class BaboonBirdPatches
{
	[HarmonyPatch("DoLOSCheck")]
	[HarmonyPostfix]
	static void DoLoSCheckPostfixPatch(BaboonBirdAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return;
		if (!EnemyDebugConfig.CheckForPosition.Value)
			return;

		Draw.Sphere(__instance.eye.position + __instance.eye.forward * 38f + __instance.eye.up * 8f, 40, color: new Color(0f, 0f, 1f, 0.1f), duration: __instance.AIIntervalTime);

		// Should be 2 * __instance.fov, but due to the sphere intersection, I'm ignoring that
		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 20), color: new Color(0f, 0f, 1f, .1f), angle: 180, duration: __instance.AIIntervalTime);

		// Should be 2 * __instance.fov, but due to the sphere intersection, I'm ignoring that
		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 10), color: new Color(1f, 0f, 0f, .1f), angle: 180, duration: __instance.AIIntervalTime);

		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 16), color: new Color(1f, 1f, 0f, .1f), angle: 2 * 80, duration: __instance.AIIntervalTime);

		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 20), color: new Color(0f, 1f, 0f, .1f), angle: 2 * 30, duration: __instance.AIIntervalTime);
	}
}
