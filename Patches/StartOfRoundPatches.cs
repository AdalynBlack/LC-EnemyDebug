using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

public class StartOfRoundPatches
{
	[HarmonyPatch(typeof(StartOfRound), "Start")]
	[HarmonyPostfix]
	static void StartPostfixPatch()
	{
		GizmoPatches.registerModels();
	}

	[HarmonyPatch(typeof(StartOfRound), "Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch()
	{
		foreach (var insideNode in RoundManager.Instance.insideAINodes)
		{
			GizmoPatches.DrawSphere(insideNode.transform.position, 0.25f, color: new Color(0.5f, 0.5f, 0.5f));
		}

		foreach (var outsideNode in RoundManager.Instance.outsideAINodes)
		{
			GizmoPatches.DrawSphere(outsideNode.transform.position, 0.25f, color: new Color(0.5f, 0.5f, 0.5f));
		}
	}
}
