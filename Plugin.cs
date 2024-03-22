using BepInEx;
using BepInEx.Logging;
using EnemyDebug.Patches;
using EnemyDebug.Config;
using HarmonyLib;

namespace EnemyDebug;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ainavt.lc.lethalconfig")]
public class EnemyDebug : BaseUnityPlugin
{
	internal static ManualLogSource HarmonyLog;

	private void Awake()
	{
		// Plugin startup logic
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

		// I prepended spaces because I hate having it right next to the colon in logs lol
		HarmonyLog = BepInEx.Logging.Logger.CreateLogSource(" EnemyDebug(Harmony)");

		EnemyDebugConfig.BindAllTo(Config);

		Harmony.CreateAndPatchAll(typeof(EnemyAIPatches));
		Harmony.CreateAndPatchAll(typeof(FlowermanAIPatches));
		Harmony.CreateAndPatchAll(typeof(SpringManPatches));
		Harmony.CreateAndPatchAll(typeof(HoarderBugPatches));
		Harmony.CreateAndPatchAll(typeof(MouthDogPatches));
		Harmony.CreateAndPatchAll(typeof(StartOfRoundPatches));
		Harmony.CreateAndPatchAll(typeof(HUDManagerPatches));
		Harmony.CreateAndPatchAll(typeof(GizmoPatches));

		EnemyDebugDynamicConfig.RegisterDynamicConfig();
	}
}
