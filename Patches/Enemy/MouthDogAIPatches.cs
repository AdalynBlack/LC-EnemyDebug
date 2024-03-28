using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(MouthDogAI))]
public class MouthDogPatches
{
	[HarmonyPatch("OnDrawGizmos")]
	[HarmonyPrefix]
	static bool OnDrawGizmosPrefixPatch(MouthDogAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return false;

		return __instance.currentBehaviourStateIndex != 0;
	}
}
