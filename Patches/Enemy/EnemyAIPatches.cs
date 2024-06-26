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

	[HarmonyPatch("GetAllPlayersInLineOfSight")]
	[HarmonyPrefix]
	static void GetAllPlayersInLineOfSightPrefix(EnemyAI __instance, float width, int range, Transform eyeObject, float proximityCheck)
	{
		if (!EnemyDebugConfig.GetAllPlayers.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;

		var eye = eyeObject != null ? eyeObject : __instance.eye;
		eye = eye == null ? __instance.transform : eye;

		if (proximityCheck > 0)
			Draw.Sphere(eye.position, proximityCheck, color: new Color(0f, 1f, 0f, 0.1f), duration: FovTimeEntries[__instance.enemyType.enemyName]);
		Draw.Cone(eye.position, eye.position + (eye.forward * range), color: new Color(0f, 1f, 0f, .1f), angle: width * 2, duration: FovTimeEntries[__instance.enemyType.enemyName]);
	}

	[HarmonyPatch("CheckLineOfSightForPlayer")]
	[HarmonyPrefix]
	static void CheckLineOfSightForPlayerPrefix(EnemyAI __instance, float width, int range, int proximityAwareness)
	{
		if (!EnemyDebugConfig.CheckForPlayer.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;
		if (proximityAwareness > 0)
			Draw.Sphere(__instance.transform.position, proximityAwareness, color: new Color(0f, 1f, 0f, 0.1f), duration: FovTimeEntries[__instance.enemyType.enemyName]);
		Draw.Cone(__instance.transform.position, __instance.transform.position + (__instance.transform.forward * range), color: new Color(0f, 1f, 0f, .1f), angle: width * 2, duration: FovTimeEntries[__instance.enemyType.enemyName]);
	}

	[HarmonyPatch("CheckLineOfSightForClosestPlayer")]
	[HarmonyPrefix]
	static void CheckLineOfSightForClosestPlayerPrefix(EnemyAI __instance, float width, int range, int proximityAwareness)
	{
		if (!EnemyDebugConfig.CheckForClosestPlayer.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;
		if (proximityAwareness > 0)
			Draw.Sphere(__instance.transform.position, proximityAwareness, color: new Color(1f, 1f, 0f, 0.1f), duration: FovTimeEntries[__instance.enemyType.enemyName]);
		Draw.Cone(__instance.transform.position, __instance.transform.position + (__instance.transform.forward * range), color: new Color(1f, 1f, 0f, .1f), angle: width * 2, duration: FovTimeEntries[__instance.enemyType.enemyName]);
	}

	[HarmonyPatch("CheckLineOfSightForPosition")]
	[HarmonyPrefix]
	static void CheckLineOfSightForPositionPrefix(EnemyAI __instance, Vector3 objectPosition, float width, int range, float proximityAwareness, Transform overrideEye)
	{
		if (!EnemyDebugConfig.CheckForPosition.Value)
			return;
		if (!__instance.debugEnemyAI)
			return;

		var eye = overrideEye != null ? overrideEye : __instance.eye;
		eye = eye == null ? __instance.transform : eye;

		Draw.Cube(objectPosition, new Vector3(0.9f, 0.9f, 0.9f), color: new Color(1f, 0f, 0f, 0.1f), duration: FovTimeEntries[__instance.enemyType.enemyName]);

		if (proximityAwareness > 0)
			Draw.Sphere(eye.position, proximityAwareness, color: new Color(1f, 0f, 0f, 0.1f), duration: FovTimeEntries[__instance.enemyType.enemyName]);

		Draw.Cone(eye.position, eye.position + (eye.forward * range), color: new Color(1f, 0f, 0f, .1f), angle: width * 2, duration: FovTimeEntries[__instance.enemyType.enemyName]);
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
	
	static internal Dictionary<string, float> FovTimeEntries = new Dictionary<string, float>();

	static ConfigEntry<bool> GetOrBindConfigEntry(EnemyAI __instance)
	{
		if(EnemyConfigs.ContainsKey(__instance.enemyType.enemyName))
			return EnemyConfigs[__instance.enemyType.enemyName];

		var settingPath = $"Enemy.{__instance.enemyType.enemyName}";

		var enabledEntry = EnemyDebugConfig.EnemyDebugFile.Bind<bool>(
				settingPath,
				"Enabled",
				false,
				"Enable debugging for this enemy");

		var checkbox = new BoolCheckBoxConfigItem(enabledEntry, requiresRestart: false);
		LethalConfigManager.AddConfigItem(checkbox);

		var fovTimeEntry = EnemyDebugConfig.EnemyDebugFile.Bind<float>(
				settingPath,
				"FoV Timeout",
				0f,
				"How long to display the FoV cone for");

		fovTimeEntry.SettingChanged += (_obj, _args) => FovTimeEntries[__instance.enemyType.enemyName] = fovTimeEntry.Value;
		FovTimeEntries[__instance.enemyType.enemyName] = fovTimeEntry.Value;
		LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(fovTimeEntry, requiresRestart: false));

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

		SubTextBuilder = new StringBuilder();
	}

	public static StringBuilder SubTextBuilder = new StringBuilder();

	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(EnemyAI __instance)
	{
		// Inform the debug patch that all draw calls are no longer from an enemy
		DebugPatches.SetEnemy(false);
		DebugPatches.SetEnemyDebug(false);

		if (EnemyDebug.Inputs.FreezeEnemiesKey.triggered)
			__instance.agent.isStopped = true;

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
				if (field == "")
					continue;

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


		nodeProps.headerText = $"{__instance.enemyType.enemyName}";
		nodeProps.subText = SubTextBuilder.ToString();
	}

	private static void ApplyDefaultStrings(EnemyAI __instance)
	{
		ApplySearchStrings(__instance);
	}

	private static bool ApplySearchStrings(EnemyAI __instance)
	{
		var search = __instance.currentSearch;

		if (search == null)
			return false;

		if (!search.inProgress)
			return false;

		SubTextBuilder
			.AppendLine($"Search width: {search.searchWidth}")
			.AppendLine($"Search precision: {search.searchPrecision}");

		if (EnemyDebugConfig.ShowExtraSearchDebug.Value)
			SubTextBuilder
				.AppendLine($"Randomized? {search.randomized}")
				.AppendLine($"Looping? {search.loopSearch}");
		
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
