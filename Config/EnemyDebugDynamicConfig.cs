using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using System.Collections;
using System.Collections.Generic;

namespace EnemyDebug.Config;

internal static class EnemyDebugDynamicConfig
{
	internal static void RegisterDynamicConfig()
	{
		LethalConfigManager.SkipAutoGen();

		AddConfigItems(new BaseConfigItem[] {
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShowPathingNodes, requiresRestart: false),
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShouldDrawWorldGizmos, requiresRestart: false),
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShowSearchedNodes, requiresRestart: false),
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShowTargetedNode, requiresRestart: false),
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShowNextTargetNode, requiresRestart: false),
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShouldDrawDefaultGizmos, requiresRestart: false),
				new BoolCheckBoxConfigItem(EnemyDebugConfig.ShouldDrawOrigin, requiresRestart: false)});
	}

	internal static void AddConfigItems(IEnumerable<BaseConfigItem> configItems)
	{
		foreach (var item in configItems)
		{
			LethalConfigManager.AddConfigItem(item);
		}
	}
}
