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

		Draw.Sphere(__instance.eye.position, 2, color: new Color(0f, 0f, 1f, 0.1f), duration: 0.2f);
		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 20), color: new Color(0f, 0f, 1f, .1f), angle: __instance.fov, duration: 0.2f);

		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 10), color: new Color(1f, 0f, 0f, .1f), angle: __instance.fov, duration: 0.2f);

		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 16), color: new Color(1f, 1f, 0f, .1f), angle: 80, duration: 0.2f);

		Draw.Cone(__instance.eye.position, __instance.eye.position + (__instance.eye.forward * 20), color: new Color(0f, 1f, 0f, .1f), angle: 30, duration: 0.2f);
	}
}
