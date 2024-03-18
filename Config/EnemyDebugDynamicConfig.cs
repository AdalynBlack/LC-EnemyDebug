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
		AddConfigItems(new BaseConfigItem[] {
				new TextInputFieldConfigItem(EnemyDebugConfig.ValidEnemiesString)});
	}

	internal static void AddConfigItems(IEnumerable<BaseConfigItem> configItems)
	{
		foreach (var item in configItems)
		{
			LethalConfigManager.AddConfigItem(item);
		}
	}
}
