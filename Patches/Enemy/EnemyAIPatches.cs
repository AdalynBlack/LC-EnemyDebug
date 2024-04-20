using BepInEx.Configuration;
using EnemyDebug.Config;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(EnemyAI))]
public class EnemyAIPatches
{
	internal static Dictionary<string, ConfigEntry<bool>> EnemyConfigs = new Dictionary<string, ConfigEntry<bool>>();

	[HarmonyPatch("CheckLineOfSightForPlayer")]
	[HarmonyPrefix]
	static void CheckLineOfSightForPlayerPrefix(EnemyAI __instance, float width, int range, int proximityAwareness)
	{
		if (!EnemyDebugConfig.ShouldDrawFov.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;
		if (proximityAwareness > 0)
			Draw.Sphere(__instance.transform.position, proximityAwareness, color: new Color(0f, 1f, 0f, 0.1f));
		Draw.Cone(__instance.transform.position, __instance.transform.position + (__instance.transform.forward * range), color: new Color(0f, 1f, 0f, .1f), angle: width);
	}

	[HarmonyPatch("CheckLineOfSightForClosestPlayer")]
	[HarmonyPrefix]
	static void CheckLineOfSightForClosestPlayerPrefix(EnemyAI __instance, float width, int range, int proximityAwareness)
	{
		if (!EnemyDebugConfig.ShouldDrawFov.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;
		if (proximityAwareness > 0)
			Draw.Sphere(__instance.transform.position, proximityAwareness, color: new Color(1f, 1f, 0f, 0.1f));
		Draw.Cone(__instance.transform.position, __instance.transform.position + (__instance.transform.forward * range), color: new Color(1f, 1f, 0f, .1f), angle: width);
	}

	[HarmonyPatch("CheckLineOfSightForPosition")]
	[HarmonyPrefix]
	static void CheckLineOfSightForPositionPrefix(EnemyAI __instance, Vector3 objectPosition, float width, int range, float proximityAwareness, Transform overrideEye)
	{
		if (!EnemyDebugConfig.ShouldDrawFov.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;

		var eye = overrideEye != null ? overrideEye : __instance.transform;

		Draw.Cube(objectPosition, new Vector3(0.9f, 0.9f, 0.9f), color: new Color(1f, 0f, 0f, 0.1f));

		if (proximityAwareness > 0)
			Draw.Sphere(eye.position, proximityAwareness, color: new Color(1f, 0f, 0f, 0.1f));

		Draw.Cone(eye.position, eye.position + (eye.forward * range), color: new Color(1f, 0f, 0f, .1f), angle: width);
	}

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

		var settingPath = $"Enemies.{enemyTypeString}.{__instance.enemyType.enemyName}";

		var enabledEntry = EnemyDebugConfig.EnemyDebugFile.Bind<bool>(
				settingPath,
				"Enabled",
				false,
				"Enable debugging for this enemy");

		var checkbox = new BoolCheckBoxConfigItem(enabledEntry, requiresRestart: false);
		LethalConfigManager.AddConfigItem(checkbox);

		var debugValuesEntry = EnemyDebugConfig.EnemyDebugFile.Bind<string>(
				settingPath,
				"Debug Values",
				"",
				"Values to debug for this enemy");

		debugValuesEntry.SettingChanged += (_obj, _args ) => ChangeDebugValues(__instance.enemyType.enemyName, debugValuesEntry);
		ChangeDebugValues(__instance.enemyType.enemyName, debugValuesEntry);

		var list = new TextInputFieldConfigItem(debugValuesEntry, requiresRestart: false);
		LethalConfigManager.AddConfigItem(list);

		return enabledEntry;
	}

	internal static Dictionary<string, List<string>> DebugValues;
	static void ChangeDebugValues(string enemyName, ConfigEntry<string> entry)
	{
		if (DebugValues == null)
			DebugValues = new Dictionary<string, List<string>>();

		DebugValues[enemyName] = new List<string>();

		var fields = entry.Value.Split(',');
		foreach (var field in fields)
			DebugValues[enemyName].Add(field);
	}

	[HarmonyDebug]
	[HarmonyPatch("CheckLineOfSightForPosition")]
	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> TranspileLineOfSightFunction(IEnumerable<CodeInstruction> instructions)
	{
		return new CodeMatcher(instructions)
			.MatchForward(false,
					new CodeMatch(OpCodes.Ldc_R4),
					new CodeMatch(OpCodes.Call))
			.SetOperandAndAdvance(0f)
			.InstructionEnumeration();
	}

	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	static void UpdatePrefixPatch(EnemyAI __instance)
	{
		// Inform the debug patch that all draw calls are currently from an enemy
		DebugPatches.SetEnemy(true);
		DebugPatches.SetEnemyDebug(__instance.debugEnemyAI);

		HeaderTextBuilder = new StringBuilder();
		SubTextBuilder = new StringBuilder();

		HeaderTextBuilder.Append($"{__instance.enemyType.enemyName}: ");
	}

	public static StringBuilder HeaderTextBuilder = new StringBuilder();
	public static StringBuilder SubTextBuilder = new StringBuilder();

	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(EnemyAI __instance)
	{
		// Inform the debug patch that all draw calls are no longer from an enemy
		DebugPatches.SetEnemy(false);
		DebugPatches.SetEnemyDebug(false);

		if (!__instance.debugEnemyAI)
			return;

		if(EnemyDebugConfig.ShouldDrawDefaultGizmos.Value)
			__instance.OnDrawGizmos();

		if(EnemyDebugConfig.ShouldDrawOrigin.Value)
			Draw.Sphere(__instance.transform.position, 0.075f, color: new Color(1f, 0f, 0f));

		var nodeProps = __instance.GetComponentInChildren<ScanNodeProperties>();

		// Scan can't reach further than 120 units (80 units long, plus 20 units offset, plus 20 units radius)
		nodeProps.maxRange = 120;
		nodeProps.minRange = 0;
		nodeProps.requiresLineOfSight = false;

		ApplyStrings(__instance, nodeProps);
	}

	private static void ApplyStrings(EnemyAI __instance, ScanNodeProperties nodeProps)
	{
		var subText = SubTextBuilder.ToString();
		if (subText == "")
			ApplyDefaultStrings(__instance);

		DebugValues.TryGetValue(__instance.enemyType.enemyName, out var fields);
		if (fields != null)
		{
			foreach (var field in fields)
			{
				FieldInfo fieldInfo;

				try {
					fieldInfo = __instance.GetType().GetField(field.Trim(), BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
				} catch (Exception e) {
					SubTextBuilder
						.AppendLine($"Unable to locate {field.Trim()}");
					EnemyDebug.HarmonyLog.LogError(e.ToString());
					continue;
				}

				var fieldType = fieldInfo.FieldType;
				var fieldValue = fieldInfo.GetValue(__instance);
				var fieldString = fieldValue.ToString();

				// For arrays, lists, etc, use String.Join() instead of Object.ToString()
				if (typeof(IEnumerable).IsAssignableFrom(fieldType))
				{
					// Do *not* touch this without retesting. Please.
					var array = (IEnumerable)fieldValue;
					fieldString = "\n\t" + String.Join(",\n\t", array.Cast<object>());
				}


				SubTextBuilder
					.Append(field.Trim())
					.Append(fieldInfo.FieldType.IsSubclassOf(typeof(bool)) ? "? " : ": ")
					.Append(fieldString)
					.Append("\n");
			}
		}


		nodeProps.headerText = HeaderTextBuilder.ToString();
		nodeProps.subText = SubTextBuilder.ToString();
		EnemyDebug.HarmonyLog.LogDebug(nodeProps.subText);
	}

	private static void ApplyDefaultStrings(EnemyAI __instance)
	{
		if (!ApplySearchStrings(__instance))
			ApplyStateStrings(__instance);
	}

	private static void ApplyStateStrings(EnemyAI __instance)
	{
		HeaderTextBuilder.Append("Current State");
		SubTextBuilder.AppendLine($"{__instance.currentBehaviourStateIndex}");
	}

	private static bool ApplySearchStrings(EnemyAI __instance)
	{
		var search = __instance.currentSearch;

		if (search == null)
			return false;

		if (!search.inProgress)
			return false;

		HeaderTextBuilder.Append("Searching");

		SubTextBuilder
			.AppendLine($"{search.nodesEliminatedInCurrentSearch}/{__instance.allAINodes.Length} nodes searched")
			.AppendLine($"Search width: {search.searchWidth}")
			.AppendLine($"Search precision: {search.searchPrecision}")
			.AppendLine($"Has finished {search.timesFinishingSearch} times")
			.AppendLine($"Randomized? {search.randomized}")
			.AppendLine($"Waiting for target? {search.waitingForTargetNode}")
			.AppendLine($"Target chosen? {search.choseTargetNode}")
			.AppendLine($"Looping? {search.loopSearch}")
			.AppendLine($"Calculating node? {search.calculatingNodeInSearch}");
		
		DrawSearchGizmos(__instance);

		return true;
	}

	private static void DrawSearchGizmos(EnemyAI __instance)
	{
		if (EnemyDebugConfig.ShowSearchedNodes.Value)
			DrawSearchedNodes(__instance);

		if (!EnemyDebugConfig.ShowTargetedNode.Value)
			return;

		var search = __instance.currentSearch;

		if (search.currentTargetNode == null)
			return;

		Draw.Sphere(search.currentTargetNode.transform.position, 0.8f, color: new Color(1f, 1f, 0f, 0.5f));
		Draw.Line(__instance.transform.position, search.currentTargetNode.transform.position, color: new Color(0.7f, 0.7f, 0.075f, 0.5f));

		if (!EnemyDebugConfig.ShowNextTargetNode.Value)
			return;

		if (search.nextTargetNode == null)
			return;
		Draw.Sphere(search.nextTargetNode.transform.position, 0.5f, color: new Color(0f, 1f, 1f, 0.3f));
		Draw.Line(search.currentTargetNode.transform.position, search.nextTargetNode.transform.position, color: new Color(0.075f, 0.7f, 0.7f, 0.3f));
	}

	private static void DrawSearchedNodes(EnemyAI __instance)
	{
		foreach (var node in __instance.allAINodes)
		{
			if (__instance.currentSearch.unsearchedNodes.Contains(node))
				continue;

			Draw.Sphere(node.transform.position, .4f, color: new Color(0f, 1f, 0f));
		}
	}
}
