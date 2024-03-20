using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EnemyDebug.Patches;

public class PlayerControllerBPatches
{
	[HarmonyPatch(typeof(PlayerControllerB), "SpeedCheat_performed")]
	[HarmonyPostfix]
	static void SpeedCheatPatch(PlayerControllerB __instance, ref InputAction.CallbackContext context)
	{
		__instance.isSpeedCheating = !__instance.isSpeedCheating;
	}
}
