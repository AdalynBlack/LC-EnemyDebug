using BepInEx.Configuration;
using EnemyDebug.Config;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(EnemyAI))]
public class EnemyAIPatches
{
	internal static Dictionary<string, ConfigEntry<bool>> EnemyConfigs = new Dictionary<string, ConfigEntry<bool>>();

	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	static void StartPostfixPatch(EnemyAI __instance)
	{
		ConfigEntry<bool> entry = GetOrBindConfigEntry(__instance);
		__instance.debugEnemyAI = entry.Value;

		if(entry.Value)
			EnemyDebug.HarmonyLog.LogDebug($"Newly added {__instance.enemyType.enemyName} is now being debugged");

		EnemyConfigs[__instance.enemyType.enemyName] = entry;
		entry.SettingChanged += (_obj, _args) =>
		{
			__instance.debugEnemyAI = entry.Value;
			EnemyDebug.HarmonyLog.LogDebug($"{__instance.enemyType.enemyName} is now {(entry.Value ? "" : "no longer ")} being debugged");
		};

	}
	
	static ConfigEntry<bool> GetOrBindConfigEntry(EnemyAI __instance)
	{
		if(EnemyConfigs.ContainsKey(__instance.enemyType.enemyName))
			return EnemyConfigs[__instance.enemyType.enemyName];

		string enemyTypeString = "Inside";

		if (__instance.enemyType.isDaytimeEnemy)
			enemyTypeString = "Daytime";
		else if (__instance.enemyType.isOutsideEnemy)
			enemyTypeString = "Outside";

		var settingPath = $"Enemies.{enemyTypeString}";
		var settingName = $"{__instance.enemyType.enemyName}";

		var entry = EnemyDebugConfig.EnemyDebugFile.Bind<bool>(
				$"Enemies.{enemyTypeString}",
				$"{__instance.enemyType.enemyName}",
				false,
				$"Enable debugging for this enemy");
		
		var checkbox = new BoolCheckBoxConfigItem(entry, requiresRestart: false);
		LethalConfigManager.AddConfigItem(checkbox);
		return entry;
	}

	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(EnemyAI __instance)
	{
		if (!__instance.debugEnemyAI)
			return;

		if(EnemyDebugConfig.ShouldDrawDefaultGizmos.Value)
			__instance.OnDrawGizmos();

		if(EnemyDebugConfig.ShouldDrawOrigin.Value)
			GizmoPatches.DrawSphere(__instance.transform.position, 0.2f, color: new Color(1f, 0f, 0f));

		var nodeProps = __instance.GetComponentInChildren<ScanNodeProperties>();

		// Scan can't reach further than 120 units (80 units long, plus 20 units offset, plus 20 units radius)
		nodeProps.maxRange = 120;
		nodeProps.minRange = 0;
		nodeProps.requiresLineOfSight = false;
		nodeProps.headerText = $"{__instance.GetType().ToString()}: ";

		SearchDebug(__instance, nodeProps);
	}

	private static void AlternateNodeProps(EnemyAI __instance, ScanNodeProperties nodeProps)
	{
		nodeProps.headerText += "Current State";
		nodeProps.subText = $"{__instance.currentBehaviourStateIndex}";
	}

	private static void SearchDebug(EnemyAI __instance, ScanNodeProperties nodeProps)
	{
		var search = __instance.currentSearch;
		if (search == null)
			return;

		if (!search.inProgress)
		{
			AlternateNodeProps(__instance, nodeProps);
			return;
		}

		nodeProps.headerText += "Searching";
		nodeProps.subText = $"{search.nodesEliminatedInCurrentSearch}/{__instance.allAINodes.Length} nodes searched\n";
		nodeProps.subText += $"Search width: {search.searchWidth}\n";
		nodeProps.subText += $"Search precision: {search.searchPrecision}\n";
		nodeProps.subText += $"Has finished {search.timesFinishingSearch} times\n";
		nodeProps.subText += $"Randomized? {search.randomized}\n";
		nodeProps.subText += $"Waiting for target? {search.waitingForTargetNode}\n";
		nodeProps.subText += $"Target chosen? {search.choseTargetNode}\n";
		nodeProps.subText += $"Looping? {search.loopSearch}\n";
		nodeProps.subText += $"Calculating node? {search.calculatingNodeInSearch}";
		
		if (EnemyDebugConfig.ShowSearchedNodes.Value)
		{
			foreach (var node in __instance.allAINodes)
			{
				if (search.unsearchedNodes.Contains(node))
					continue;

				GizmoPatches.DrawSphere(node.transform.position, .4f, color: new Color(0f, 1f, 0f));
			}
		}

		if (!EnemyDebugConfig.ShowTargetedNode.Value)
			return;

		if (search.currentTargetNode == null)
			return;
		GizmoPatches.DrawSphere(search.currentTargetNode.transform.position, 0.8f, color: new Color(1f, 1f, 0f, 0.5f));
		GizmoPatches.DrawLine(__instance.transform.position, search.currentTargetNode.transform.position, color: new Color(0.7f, 0.7f, 0.2f, 0.5f));

		if (!EnemyDebugConfig.ShowNextTargetNode.Value)
			return;

		if (search.nextTargetNode == null)
			return;
		GizmoPatches.DrawSphere(search.nextTargetNode.transform.position, 0.5f, color: new Color(0f, 1f, 1f, 0.3f));
		GizmoPatches.DrawLine(search.currentTargetNode.transform.position, search.nextTargetNode.transform.position, color: new Color(0.2f, 0.7f, 0.7f, 0.3f));
	}
}
