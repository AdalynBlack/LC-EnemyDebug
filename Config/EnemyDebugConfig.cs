using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EnemyDebug.Config;

public static class EnemyDebugConfig
{
	public static ConfigFile EnemyDebugFile;

	// World
	public static ConfigEntry<bool> ShowPathingNodes;

	// Search
	public static ConfigEntry<bool> ShowSearchedNodes;
	public static ConfigEntry<bool> ShowTargetedNode;
	public static ConfigEntry<bool> ShowNextTargetNode;

	// Misc
	public static ConfigEntry<bool> ShouldDrawDefaultGizmos;

	//Advanced
	public static ConfigEntry<bool> ShouldDrawOrigin;


	public static void BindAllTo(ConfigFile config)
	{
		EnemyDebugFile = config;

		// World
		ShowPathingNodes = EnemyDebugFile.Bind<bool>(
				"World",
				"Show Pathing Nodes",
				false);

		// Search
		ShowSearchedNodes = EnemyDebugFile.Bind<bool>(
				"Search",
				"Show Searched Nodes",
				false);

		ShowTargetedNode = EnemyDebugFile.Bind<bool>(
				"Search",
				"Show Currently Targeted Node",
				true);

		ShowNextTargetNode = EnemyDebugFile.Bind<bool>(
				"Search",
				"Show Next Target Node",
				false);

		// Misc
		ShouldDrawDefaultGizmos = EnemyDebugFile.Bind<bool>(
				"Misc",
				"Should Draw Default Gizmos?",
				true);

		// Advanced
		ShouldDrawOrigin = EnemyDebugFile.Bind<bool>(
				"Advanced",
				"Should Draw Origin?",
				false);

		// Remove now unused value
		EnemyDebugFile.Remove(EnemyDebugFile.Bind<string>(
				"Enemies",
				"Enemies to Debug",
				"").Definition);
	}
}
