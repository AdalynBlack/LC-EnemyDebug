using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

public class MouthDogPatches
{
	[HarmonyPatch(typeof(MouthDogAI), "OnDrawGizmos")]
	[HarmonyPrefix]
	static bool OnDrawGizmosPrefixPatch(MouthDogAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return false;

		return __instance.currentBehaviourStateIndex != 0;
	}
}
