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
	public static ConfigEntry<bool> ShouldDrawWorldGizmos;

	// Search
	public static ConfigEntry<bool> ShowSearchedNodes;
	public static ConfigEntry<bool> ShowTargetedNode;
	public static ConfigEntry<bool> ShowNextTargetNode;

	// FoV
	public static ConfigEntry<bool> GetAllPlayers;
	public static ConfigEntry<bool> CheckForPlayer;
	public static ConfigEntry<bool> CheckForClosestPlayer;
	public static ConfigEntry<bool> CheckForPosition;

	// Misc
	public static ConfigEntry<bool> ShouldDrawDefaultGizmos;

	//Advanced
	public static ConfigEntry<bool> ShouldDrawOrigin;
	public static ConfigEntry<bool> ShowExtraSearchDebug;

	public static void BindAllTo(ConfigFile config)
	{
		EnemyDebugFile = config;

		// World
		ShowPathingNodes = EnemyDebugFile.Bind<bool>(
				"World",
				"Show Pathing Nodes",
				false);

		ShouldDrawWorldGizmos = EnemyDebugFile.Bind<bool>(
				"World",
				"Should Draw Default World Gizmos",
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
		
		// FoV
		GetAllPlayers = EnemyDebugFile.Bind<bool>(
				"FoV",
				"Display GetAllPlayers FoV? (green)",
				true);

		CheckForPlayer = EnemyDebugFile.Bind<bool>(
				"FoV",
				"Display CheckForPlayer FoV? (green)",
				true);

		CheckForClosestPlayer = EnemyDebugFile.Bind<bool>(
				"FoV",
				"Display CheckForClosestPlayer FoV? (yellow)",
				true);

		CheckForPosition = EnemyDebugFile.Bind<bool>(
				"FoV",
				"Display CheckForPosition FoV? (red)",
				true);

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

		ShowExtraSearchDebug = EnemyDebugFile.Bind<bool>(
				"Advanced",
				"Show Advanced Search Information?",
				false);

		// Remove now unused value
		EnemyDebugFile.Remove(EnemyDebugFile.Bind<string>(
				"Enemies",
				"Enemies to Debug",
				"").Definition);
	}
}
