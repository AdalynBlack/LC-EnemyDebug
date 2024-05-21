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

	public static void RegisterMeshes(Material mat)
	{
		var sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		SphereMesh = Object.Instantiate(sphereObject.GetComponent<MeshFilter>().mesh);
		GameObject.Destroy(sphereObject);

		var cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		CubeMesh = Object.Instantiate(cubeObject.GetComponent<MeshFilter>().mesh);
		GameObject.Destroy(cubeObject);

		var material = Object.Instantiate(mat);
		material.enableInstancing = true;
		material.SetTexture("_BaseColorMap", null);
		material.SetTexture("_MainTex", null);
		material.SetTexture("_NormalMap", null);

		MaterialProperties = new MaterialPropertyBlock();
		DebugRenderParams = new RenderParams(material) {matProps = MaterialProperties};
	}

	public static void DrawAll()
	{
		DrawAs(SphereData, SphereMesh);
		DrawAs(CubeData, CubeMesh);

		foreach (float angle in ConeData.Keys)
		{
			DrawAs(ConeData[angle], ConeMesh[angle]);
		}
	}

	private static void DrawAs(Dictionary<Color, List<RenderData>> data, Mesh mesh)
	{
		foreach (Color color in data.Keys)
		{
			MaterialProperties.SetColor("_BaseColor", color);
			MaterialProperties.SetColor("_Color", color);
			MaterialProperties.SetColor("_EmissiveColor", color);

			foreach (RenderData datum in data[color])
			{
				Graphics.RenderMesh(DebugRenderParams, mesh, 0, datum.objectToWorld);
			}

			// For some reason, this doesn't work. Unity docs say it should, but it doesn't
			//Graphics.RenderMeshInstanced(DebugRenderParams, mesh, 0, data[color].ToArray());

			data[color].RemoveAll(datum => datum.duration <= 0f);
			data[color].ForEach(datum => datum.Decrement(Time.deltaTime));
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

	private static Dictionary<float, Dictionary<Color, List<RenderData>>> ConeData = new Dictionary<float, Dictionary<Color, List<RenderData>>>();
	private static Dictionary<float, Mesh> ConeMesh = new Dictionary<float, Mesh>();

	public static void Cone(Vector3 start, Vector3 end, Color? color = null, float angle = 1.0f, float duration = 0.0f, bool depthTest = true)
	{
		Color setColor = color ?? Color.green;

		var offset = end - start;
		var length = offset.magnitude;

		if(length <= 0)
			return;

		ConeData.TryGetValue(angle, out var data);
		if (data == null)
		{
			ConeData[angle] = data = new Dictionary<Color, List<RenderData>>();
			ConeMesh[angle] = GenerateCone(angle);
		}

		data.TryGetValue(setColor, out var dataList);
		if (dataList == null)
		{
			ConeData[angle][setColor] = new List<RenderData>();
		}

		var lookRotation = Quaternion.LookRotation(offset);
		ConeData[angle][setColor].Add(new RenderData {
				objectToWorld = Matrix4x4.TRS(
					start,
					lookRotation,
					new Vector3(length, length, length)),
				duration = duration,
				depthTest = depthTest});
	}

	const float SPHERE_RINGS_COUNT = 32f;
	const float SPHERE_LINES_COUNT = 32f;

	public static Mesh GenerateCone(float angle)
	{
		Mesh coneMesh = new Mesh();

		// Ring count has to be 2 or higher or it breaks because I don't get paid enough to fix it :D
		int ringsCount = Mathf.Max(2, (int)(SPHERE_RINGS_COUNT*(angle/360f)) + 1);
		int vertCount = ringsCount * (int)SPHERE_LINES_COUNT + 2;
		Vector3[] verts = new Vector3[vertCount];
		int[] indices = new int[6 * ((ringsCount + 1) * (int)SPHERE_LINES_COUNT)];

		EnemyDebug.HarmonyLog.LogDebug($"Generating new cone with {ringsCount} rings and {vertCount} vertices");

		// Set the centers of both ends of the cone
		verts[0] = new Vector3(0f, 0f, 1f);
		verts[vertCount - 1] = new Vector3(0f, 0f, 0f);

		for (int ring = 1; ring < (ringsCount + 1); ring++)
		{
			// Figure out where in the array to edit for this ring
			int vertOffset = (ring - 1) * (int)SPHERE_LINES_COUNT + 1;

			// Figure out the distance and size of the vertex ring
			float ringAngle = Mathf.Deg2Rad * angle * ((float)ring / ringsCount) / 2f;
			float ringDistance = Mathf.Cos(ringAngle);
			float ringSize = Mathf.Sin(ringAngle);

			for (int vert = 0; vert < SPHERE_LINES_COUNT; vert++)
			{
				// Find the angle of this vertex
				float vertAngle = -2 * Mathf.PI * (vert / SPHERE_LINES_COUNT);

				// Get the exact index to modify for this vertex
				int currentVert = vertOffset + vert;
				verts[currentVert] = new Vector3(Mathf.Cos(vertAngle), Mathf.Sin(vertAngle), ringDistance / ringSize) * ringSize;

				// Get the index in the indices array to modify for this vertex
				int indexOffset = 6 * vertOffset + (vert * 6) - (3 * (int)SPHERE_LINES_COUNT);

				// Precalcualte the next vertex in the ring, accounting for wrapping
				var nextVert = (int)(vertOffset + ((vert + 1) % SPHERE_LINES_COUNT));

				// If we're not on the first ring (yes I started at 1 to make the math easier)
				// Draw the triangles for the quad
				if (ring != 1)
				{
					indices[indexOffset] = currentVert - (int)SPHERE_LINES_COUNT;
					indices[indexOffset + 1] = nextVert;
					indices[indexOffset + 2] = currentVert;
					indices[indexOffset + 3] = nextVert - (int)SPHERE_LINES_COUNT;
					indices[indexOffset + 4] = nextVert;
					indices[indexOffset + 5] = currentVert - (int)SPHERE_LINES_COUNT;
				} else {
					// We're on ring 1, offset our index to use 3 indices instead of 6 so we can use tris
					indexOffset += 3 * (int)SPHERE_LINES_COUNT;
					indexOffset /= 2;
					// Connect to first index if we're on the innermost ring
					indices[indexOffset] = 0;
					indices[indexOffset + 1] = nextVert;
					indices[indexOffset + 2] = currentVert;
				}

				if (ring == ringsCount)
				{
					// Go forwards one layer if we're on the last ring
					indexOffset += (int)SPHERE_LINES_COUNT * 6;
					// Connect to last index if we're on the outermost ring
					indices[indexOffset] = vertCount - 1;
					indices[indexOffset + 1] = currentVert;
					indices[indexOffset + 2] = nextVert;
				}
			}
		}

		coneMesh.SetVertices(verts.ToList());
		coneMesh.SetIndices(indices.ToList(), MeshTopology.Triangles, 0);

		return coneMesh;
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
