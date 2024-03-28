using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches;

[HarmonyPatch(typeof(Gizmos))]
public class GizmoPatches
{
	private static Mesh sphere;
	private static Mesh cube;

	private static MaterialPropertyBlock matProps;
	private static RenderParams debugRenderParams;

	public static void registerModels()
	{
		var sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere = Object.Instantiate(sphereObject.GetComponent<MeshFilter>().mesh);
		GameObject.Destroy(sphereObject);

		var cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube = Object.Instantiate(cubeObject.GetComponent<MeshFilter>().mesh);
		GameObject.Destroy(cubeObject);

		var material = new Material(Shader.Find("HDRP/Unlit"));

		matProps = new MaterialPropertyBlock();
		debugRenderParams = new RenderParams(material) {matProps = matProps};
	}

	[HarmonyPatch("DrawSphere")]
	[HarmonyPostfix]
	public static void DrawSpherePatch(Vector3 center, float radius)
	{
		DrawSphere(center, radius, color: null);
	}

	public static void DrawSphere(Vector3 center, float radius, Color? color = null)
	{
		Color setColor = color ?? new Color(1f, 0f, 1f);
		matProps.SetColor("_UnlitColor", setColor);

		Graphics.RenderMesh(
				debugRenderParams, sphere, 0,
				Matrix4x4.TRS(
					center,
					Quaternion.identity,
					new Vector3(radius * 2, radius * 2, radius * 2)));
	}

	[HarmonyPatch("DrawCube")]
	[HarmonyPostfix]
	public static void DrawCubePatch(Vector3 center, Vector3 size)
	{
		DrawCube(center, size, color: null);
	}

	public static void DrawCube(Vector3 center, Vector3 size, Color? color = null)
	{
		Color setColor = color ?? new Color(1f, .5f, 0f);
		matProps.SetColor("_UnlitColor", setColor);

		Graphics.RenderMesh(
				debugRenderParams, cube, 0,
				Matrix4x4.TRS(
					center,
					Quaternion.identity,
					size));
	}

	[HarmonyPatch("DrawLine")]
	[HarmonyPostfix]
	public static void DrawLinePatch(Vector3 from, Vector3 to)
	{
		DrawLine(from, to, color: null);
	}

	public static void DrawLine(Vector3 from, Vector3 to, Color? color = null)
	{
		Color setColor = color ?? new Color(0f, 1f, 0f);
		matProps.SetColor("_UnlitColor", setColor);

		var offset = to - from;
		var length = offset.magnitude;

		if(length <= 0)
			return;

		var lookRotation = Quaternion.LookRotation(offset);
		Graphics.RenderMesh(
				debugRenderParams, cube, 0,
				Matrix4x4.TRS(
					(from + to)/2,
					lookRotation,
					new Vector3(.2f, .2f, length)));
	}
}
