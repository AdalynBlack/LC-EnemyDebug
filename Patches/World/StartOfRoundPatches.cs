using EnemyDebug.Config;
using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches.World;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatches
{
	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch()
	{
		if(!EnemyDebugConfig.ShowPathingNodes.Value)
			return;

		foreach (var insideNode in RoundManager.Instance.insideAINodes)
		{
			Draw.Sphere(insideNode.transform.position, 0.25f, color: new Color(0.5f, 0.5f, 0.5f));
		}

		foreach (var outsideNode in RoundManager.Instance.outsideAINodes)
		{
			Draw.Sphere(outsideNode.transform.position, 0.25f, color: new Color(0.5f, 0.5f, 0.5f));
		}
	}
}
