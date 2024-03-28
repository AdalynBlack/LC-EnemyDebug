using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

[HarmonyPatch(typeof(Gizmos))]
public class GizmoPatches
{
	[HarmonyPatch("DrawSphere")]
	[HarmonyPostfix]
	public static void DrawSpherePatch(Vector3 center, float radius)
	{
		Draw.Sphere(center, radius, color: null);
	}

	[HarmonyPatch("DrawCube")]
	[HarmonyPostfix]
	public static void DrawCubePatch(Vector3 center, Vector3 size)
	{
		Draw.Cube(center, size, color: null);
	}

	[HarmonyPatch("DrawLine")]
	[HarmonyPostfix]
	public static void DrawLinePatch(Vector3 from, Vector3 to)
	{
		Draw.Line(from, to, color: null);
	}
}
