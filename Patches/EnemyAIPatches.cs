using EnemyDebug.Config;
using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

public class EnemyAIPatches
{
	[HarmonyPatch(typeof(EnemyAI), "Start")]
	[HarmonyPostfix]
	static void StartPostfixPatch(EnemyAI __instance)
	{
		string typeString = __instance.GetType().ToString();
		bool shouldDebug = EnemyDebugConfig.ValidEnemies.Contains(typeString);
		EnemyDebug.HarmonyLog.LogDebug($"Should {typeString} be debugged? {(shouldDebug ? "Yes" : "No")}");
		__instance.debugEnemyAI = shouldDebug;
	}

	[HarmonyPatch(typeof(EnemyAI), "Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(EnemyAI __instance)
	{
		__instance.debugEnemyAI = EnemyDebugConfig.ValidEnemies.Contains(__instance.GetType().ToString());

		if(!__instance.debugEnemyAI)
			return;

		__instance.OnDrawGizmos();

		GizmoPatches.DrawSphere(__instance.transform.position, 0.2f, color: new Color(1f, 0f, 0f));

		var search = __instance.currentSearch;
		if(search == null)
			return;

		if(!search.inProgress)
			return;

		foreach(var node in __instance.allAINodes)
		{
			if(search.unsearchedNodes.Contains(node))
				continue;

			GizmoPatches.DrawSphere(node.transform.position, .4f, color: new Color(0f, 1f, 0f));
		}

		if(search.currentTargetNode == null)
			return;
		GizmoPatches.DrawSphere(search.currentTargetNode.transform.position, 0.8f, color: new Color(1f, 1f, 0f, 0.5f));
		GizmoPatches.DrawLine(__instance.transform.position, search.currentTargetNode.transform.position, color: new Color(0.7f, 0.7f, 0.2f, 0.5f));

		if(search.nextTargetNode == null)
			return;
		GizmoPatches.DrawSphere(search.nextTargetNode.transform.position, 0.5f, color: new Color(0f, 1f, 1f, 0.3f));
		GizmoPatches.DrawLine(search.currentTargetNode.transform.position, search.nextTargetNode.transform.position, color: new Color(0.2f, 0.7f, 0.7f, 0.3f));
	}
}
