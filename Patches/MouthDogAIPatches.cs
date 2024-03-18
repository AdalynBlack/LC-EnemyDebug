using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

public class MouthDogPatches
{
	[HarmonyPatch(typeof(MouthDogAI), "OnDrawGizmos")]
	[HarmonyPostfix]
	static bool OnDrawGizmosPrefixPatch(MouthDogAI __instance)
	{
		return __instance.currentBehaviourStateIndex != 0;
	}
}
