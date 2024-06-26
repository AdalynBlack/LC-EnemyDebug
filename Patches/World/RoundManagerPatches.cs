using EnemyDebug.Config;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyDebug.Patches.World;

public class RoundManagerPatches
{
	[HarmonyPatch(typeof(RoundManager), "SpawnOutsideHazards")]
	[HarmonyPostfix]
	static void SpawnOutsideHazardsPatch(RoundManager __instance)
	{
		var triangulation = NavMesh.CalculateTriangulation();

		var rawMesh = new Mesh();
		rawMesh.SetVertices(triangulation.vertices);

		var indices = new List<int>();
		for (int i = 0; i < triangulation.indices.Length / 3; i++)
		{
			if ((triangulation.areas[i] & EnemyDebugConfig.NavmeshBitmask.Value) != 0)
			{
				indices.Add(triangulation.indices[i * 3]);
				indices.Add(triangulation.indices[i * 3 + 1]);
				indices.Add(triangulation.indices[i * 3 + 2]);
			}
		}

		rawMesh.SetIndices(indices, MeshTopology.Triangles, 0, true);

		var meshFilter = __instance.gameObject.AddComponent<MeshFilter>();
		__instance.gameObject.AddComponent<MeshRenderer>();

		meshFilter.mesh = rawMesh;
	}

	static List<Mesh> GetNavmeshMeshes()
	{
		var triangulation = NavMesh.CalculateTriangulation();

		var meshes = new List<Mesh>();

		meshes.Add(GetNavmeshMeshFromTriangulation(triangulation));
		for (int i = 1; i <= 0x200; i *= 2)
		{
			meshes.Add(GetNavmeshMeshFromTriangulation(triangulation, i));
		}

		return meshes;
	}

	static Mesh GetNavmeshMeshFromTriangulation(NavMeshTriangulation triangulation, int bitMask = -1)
	{
		var rawMesh = new Mesh();
		rawMesh.SetVertices(triangulation.vertices);

		var indices = new List<int>();
		for (int i = 0; i < triangulation.indices.Length / 3; i++)
		{
			if (bitMask == -1 || (triangulation.areas[i] & bitMask) != 0)
			{
				indices.Add(triangulation.indices[i * 3]);
				indices.Add(triangulation.indices[i * 3 + 1]);
				indices.Add(triangulation.indices[i * 3 + 2]);
			}
		}

		rawMesh.SetIndices(indices, MeshTopology.Triangles, 0, true);

		return rawMesh;
	}
}
