using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

public class FlowermanPatches
{
	[HarmonyPatch(typeof(FlowermanAI), "Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(FlowermanAI __instance)
	{
		if(__instance.favoriteSpot == null)
			return;

		GizmoPatches.DrawSphere(__instance.favoriteSpot.position, 1f, color: new Color(0f, 0f, 1f));
	}
}
