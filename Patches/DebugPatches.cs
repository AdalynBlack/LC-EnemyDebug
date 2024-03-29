using EnemyDebug.Config;
using HarmonyLib;
using System;
using UnityEngine;

namespace EnemyDebug.Patches;

[HarmonyPatch(typeof(Debug))]
public class DebugPatches
{
	static bool IsEnemy = false;
	static bool IsDebugEnemy = false;

	[HarmonyPatch("DrawLine", new Type[] {typeof(Vector3), typeof(Vector3), typeof(Color), typeof(float), typeof(bool)})]
	[HarmonyPrefix]
	public static void DrawLinePatch(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
	{
		if (IsEnemy && !IsDebugEnemy)
			return;
		else if (!IsEnemy && !EnemyDebugConfig.ShouldDrawWorldGizmos.Value)
			return;

		EnemyDebug.HarmonyLog.LogDebug("Drawing a line");
		Draw.Line(start, end, color, duration, depthTest);
	}

	[HarmonyPatch("DrawRay", new Type[] {typeof(Vector3), typeof(Vector3), typeof(Color), typeof(float), typeof(bool)})]
	[HarmonyPrefix]
	public static void DrawRayPatch(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest)
	{
		if (IsEnemy && !IsDebugEnemy)
			return;
		else if (!IsEnemy && !EnemyDebugConfig.ShouldDrawWorldGizmos.Value)
			return;

		EnemyDebug.HarmonyLog.LogDebug("Drawing a ray");
		// There's probably a better way to do this, but this works too lol
		Draw.Line(start, start + (dir * 1000), color, duration, depthTest);
	}

	public static void SetEnemy(bool isEnemy)
	{
		IsEnemy = isEnemy;
	}

	public static void SetEnemyDebug(bool isDebugEnemy)
	{
		IsDebugEnemy = isDebugEnemy;
	}
}
