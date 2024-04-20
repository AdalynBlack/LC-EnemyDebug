using HarmonyLib;
using UnityEngine;

namespace EnemyDebug.Patches.Enemy;

[HarmonyPatch(typeof(ButlerEnemyAI))]
public class ButlerEnemyPatches
{
	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	static void UpdatePostfixPatch(ButlerEnemyAI __instance)
	{
		if(!__instance.debugEnemyAI)
			return;

		var timeOfLastSeenPlayers = (float[])AccessTools.Field(typeof(ButlerEnemyAI), "timeOfLastSeenPlayers").GetValue(__instance);
		var timeSinceCheckingForMultiplePlayers = (float)AccessTools.Field(typeof(ButlerEnemyAI), "timeSinceCheckingForMultiplePlayers").GetValue(__instance);
		var seenPlayers = (bool[])AccessTools.Field(typeof(ButlerEnemyAI), "seenPlayers").GetValue(__instance);
		for (int i = 0; i < timeOfLastSeenPlayers.Length; i++)
		{
			if (StartOfRound.Instance.allPlayerScripts[i] == null)
				continue;

			if (StartOfRound.Instance.allPlayerScripts[i].gameplayCamera == null)
				continue;

			if (StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform == null)
				continue;

			if (__instance.targetPlayer == null)
				continue;

			var cameraTransform = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform;
			if (i == (int)__instance.targetPlayer.playerClientId)
			{
				Draw.Sphere(cameraTransform.position + cameraTransform.forward + ((-cameraTransform.right + cameraTransform.up) * 0.5f), 0.1f, color: new Color(0f, 1f, 0f, 1f));
				Draw.Cube(cameraTransform.position - Vector3.up, new Vector3(2, 3, 2), color: new Color(0f, 1f, 0f, 1f));
			}

			if (!seenPlayers[i])
				continue;

			var timeSinceSeeing = Time.realtimeSinceStartup - timeOfLastSeenPlayers[i];

			float timeout = 6;
			if (i == (int)__instance.targetPlayer.playerClientId)
				timeout = 12;

			if (timeSinceCheckingForMultiplePlayers > 2f && timeSinceCheckingForMultiplePlayers < 4f)
				timeout = 2;

			if (timeSinceSeeing > timeout)
				continue;

			var normalizedTime = 1 - (timeSinceSeeing / timeout);

			Draw.Cube(cameraTransform.position - Vector3.up, new Vector3(2, 3, 2), color: new Color(normalizedTime, timeout == 12 ? 1f : 0f, 0f, 1f));
			Draw.Sphere(cameraTransform.position + cameraTransform.forward + ((cameraTransform.right + cameraTransform.up) * 0.5f), 0.1f, color: new Color(normalizedTime, 0f, 0f, 1f));
		}
	}
}
