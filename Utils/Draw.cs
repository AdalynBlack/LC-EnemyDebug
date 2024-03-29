using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EnemyDebug;

public static class Draw
{
	private static Mesh SphereMesh;
	private static Mesh CubeMesh;

	private static MaterialPropertyBlock MaterialProperties;
	private static RenderParams DebugRenderParams;

	public static void RegisterMeshes()
	{
		var sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		SphereMesh = Object.Instantiate(sphereObject.GetComponent<MeshFilter>().mesh);
		GameObject.Destroy(sphereObject);

		var cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		CubeMesh = Object.Instantiate(cubeObject.GetComponent<MeshFilter>().mesh);
		GameObject.Destroy(cubeObject);

		var material = new Material(Shader.Find("HDRP/Unlit"));
		material.enableInstancing = true;
		material.renderQueue = 3000;

		MaterialProperties = new MaterialPropertyBlock();
		DebugRenderParams = new RenderParams(material) {matProps = MaterialProperties};
	}

	public static void DrawAll()
	{
		foreach (Color color in SphereData.Keys)
		{
			EnemyDebug.HarmonyLog.LogDebug($"Drawing {SphereData[color].Count} spheres in ({color})");

			MaterialProperties.SetColor("_UnlitColor", color);

			foreach (RenderData data in SphereData[color])
			{
				Graphics.RenderMesh(DebugRenderParams, SphereMesh, 0, data.objectToWorld);
			}

			// For some reason, this doesn't work. Unity docs say it should, but it doesn't
			//Graphics.RenderMeshInstanced(DebugRenderParams, SphereMesh, 0, SphereData[color].ToArray());

			SphereData[color].RemoveAll(data => data.duration <= 0f);
			SphereData[color].ForEach(data => data.Decrement(Time.deltaTime));
		}

		foreach (Color color in CubeData.Keys)
		{
			EnemyDebug.HarmonyLog.LogDebug($"Drawing {CubeData[color].Count} cubes in ({color})");

			MaterialProperties.SetColor("_UnlitColor", color);

			foreach (RenderData data in CubeData[color])
			{
				Graphics.RenderMesh(DebugRenderParams, CubeMesh, 0, data.objectToWorld);
			}

			// For some reason, this doesn't work. Unity docs say it should, but it doesn't
			//Graphics.RenderMeshInstanced(DebugRenderParams, CubeMesh, 0, CubeData[color].ToArray());

			CubeData[color].RemoveAll(data => data.duration <= 0f);
			CubeData[color].ForEach(data => data.Decrement(Time.deltaTime));
		}
	}

	private static Dictionary<Color, List<RenderData>> SphereData = new Dictionary<Color, List<RenderData>>();

	public static void Sphere(Vector3 center, float radius, Color? color = null, float duration = 0.0f, bool depthTest = true)
	{
		Color setColor = color ?? Color.magenta;

		SphereData.TryGetValue(setColor, out var dataList);
		if (dataList == null)
			SphereData[setColor] = new List<RenderData>();

		SphereData[setColor].Add(new RenderData {
				objectToWorld = Matrix4x4.TRS(
					center,
					Quaternion.identity,
					new Vector3(radius * 2, radius * 2, radius * 2)),
				duration = duration,
				depthTest = depthTest});
	}

	private static Dictionary<Color, List<RenderData>> CubeData = new Dictionary<Color, List<RenderData>>();

	public static void Cube(Vector3 center, Vector3 size, Color? color = null, float duration = 0.0f, bool depthTest = true)
	{
		Color setColor = color ?? new Color(1f, .5f, 0f);

		CubeData.TryGetValue(setColor, out var dataList);
		if (dataList == null)
			CubeData[setColor] = new List<RenderData>();

		CubeData[setColor].Add(new RenderData {
				objectToWorld = Matrix4x4.TRS(
					center,
					Quaternion.identity,
					size),
				duration = duration,
				depthTest = depthTest});
	}

	public static void Line(Vector3 start, Vector3 end, Color? color = null, float duration = 0.0f, bool depthTest = true)
	{
		Color setColor = color ?? Color.green;

		var offset = end - start;
		var length = offset.magnitude;

		if(length <= 0)
			return;

		CubeData.TryGetValue(setColor, out var dataList);
		if (dataList == null)
			CubeData[setColor] = new List<RenderData>();

		var lookRotation = Quaternion.LookRotation(offset);
		CubeData[setColor].Add(new RenderData {
				objectToWorld = Matrix4x4.TRS(
					(start + end)/2,
					lookRotation,
					new Vector3(.2f, .2f, length)),
				duration = duration,
				depthTest = depthTest});
	}

	public class RenderData
	{
		// Must be named exactly this because unity
		public Matrix4x4 objectToWorld;
		public float duration;
		public bool depthTest;

		public void Decrement(float deltaTime)
		{
			this.duration -= deltaTime;
		}
	}
}
