using BepInEx;
using BepInEx.Logging;
using EnemyDebug.Patches;
using EnemyDebug.Patches.Enemy;
using EnemyDebug.Patches.UI;
using EnemyDebug.Patches.World;
using EnemyDebug.Config;
using EnemyDebug.Input;
using HarmonyLib;

namespace EnemyDebug;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.rune580.LethalCompanyInputUtils")]
[BepInDependency("ainavt.lc.lethalconfig")]
public class EnemyDebug : BaseUnityPlugin
{
	internal static ManualLogSource HarmonyLog;
	internal static EnemyDebugInputs Inputs;

	private void Awake()
	{
		// Plugin startup logic
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loading...");

		// I prepended spaces because I hate having it right next to the colon in logs lol
		HarmonyLog = BepInEx.Logging.Logger.CreateLogSource(" EnemyDebug(Harmony)");

		Inputs = new EnemyDebugInputs();

		EnemyDebugConfig.BindAllTo(Config);

		Harmony.CreateAndPatchAll(typeof(BaboonBirdPatches));
		Harmony.CreateAndPatchAll(typeof(FlowermanAIPatches));
		Harmony.CreateAndPatchAll(typeof(HoarderBugPatches));
		Harmony.CreateAndPatchAll(typeof(MouthDogPatches));
		Harmony.CreateAndPatchAll(typeof(SpringManPatches));
		Harmony.CreateAndPatchAll(typeof(ButlerEnemyPatches));
		Harmony.CreateAndPatchAll(typeof(EnemyAIPatches));

		Harmony.CreateAndPatchAll(typeof(StartOfRoundPatches));

		Harmony.CreateAndPatchAll(typeof(HUDManagerPatches));

		Harmony.CreateAndPatchAll(typeof(DebugPatches));
		Harmony.CreateAndPatchAll(typeof(GizmoPatches));

		EnemyDebugDynamicConfig.RegisterDynamicConfig();

		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	}
}
