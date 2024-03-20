using HarmonyLib;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EnemyDebug.Patches;

public class HUDManagerPatches
{
	[HarmonyPatch(typeof(HUDManager), "NodeIsNotVisible")]
	[HarmonyPrefix]
	static bool NodeVisibilityPatch(HUDManager __instance, ScanNodeProperties node, int elementIndex, ref bool __result)
	{
		var subtext = __instance.scanElements[elementIndex].gameObject.GetComponentsInChildren<TextMeshProUGUI>()[1];
		if(subtext.textInfo == null)
			return false;

		var subtextBox = __instance.scanElements[elementIndex].gameObject.GetComponentsInChildren<Image>()[3];
		EnemyDebug.HarmonyLog.LogDebug($"{subtextBox.gameObject}");
		var subtextTransform = subtextBox.GetComponent<RectTransform>();

		subtext.verticalAlignment = VerticalAlignmentOptions.Top;

		var lineCount = subtext.textInfo.lineCount;
		EnemyDebug.HarmonyLog.LogDebug($"Lines to fit: {lineCount}");
		subtextTransform.localScale = new Vector3(1, lineCount, 1);
		subtextTransform.anchoredPosition = new Vector3(145.64f, 11.12f - (7.27f * lineCount), 1);

		__instance.scanElements[elementIndex].gameObject.SetActive(
				// Reflection to access __instance.nodesOnScreen and run the "Contains" method using node
				((List<ScanNodeProperties>)(AccessTools.Field(typeof(HUDManager), "nodesOnScreen").GetValue(__instance)))
				.Contains(node));

		__result = false;
		return false;
	}

	[HarmonyPatch(typeof(HUDManager), "AssignNewNodes")]
	[HarmonyPrefix]
	static void AssignNewNodesPatch(HUDManager __instance)
	{
		var scanNodes = (Dictionary<RectTransform, ScanNodeProperties>)typeof(HUDManager).GetField("scanNodes", System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Instance).GetValue(__instance);

		scanNodes.Clear();
	}
}
