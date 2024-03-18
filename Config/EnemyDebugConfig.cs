using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EnemyDebug.Config;

public static class EnemyDebugConfig
{
	public static ConfigFile EnemyDebugFile;

	internal static ConfigEntry<string> ValidEnemiesString;

	private static string lastEnemiesString = "";

	private static List<string> _validEnemies;
	public static List<string> ValidEnemies
	{
		get {
			if(lastEnemiesString == ValidEnemiesString.Value)
				return _validEnemies;
			
			lastEnemiesString = ValidEnemiesString.Value;
			_validEnemies = lastEnemiesString.Split(", ").ToList();

			return _validEnemies;
		}
		private set {
			EnemyDebug.HarmonyLog.LogDebug("Valid enemies list modification detected");
			_validEnemies = value;
		}
	}

	public static void BindAllTo(ConfigFile config)
	{
		EnemyDebugFile = config;

		ValidEnemiesString = EnemyDebugFile.Bind<string>(
				"Enemies",
				"Enemies to Debug",
				"BaboonBirdAI, BlobAI, CentipedeAI, CrawlerAI, DressGirlAI, FlowermanAI, ForestGiantAI, HoarderBugAI, JesterAI, MaskedPlayerEnemy, MouthDogAI, NutcrakcerEnemyAI, PufferAI, SandSpiderAI, SandWormAI, SpringManAI",
				new ConfigDescription(
					"A comma separated list of enemies to enable debugging on. Allowed values are [BaboonBirdAI, BlobAI, CentipedeAI, CrawlerAI, DocileLocustBeesAI, DoublewingAI, DressGirlAI, FlowermanAI, ForestGiantAI, HoarderBugAI, JesterAI, LassoManAI, MaskedPlayerEnemy, MouthDogAI, NutcrakcerEnemyAI, PufferAI, RedLocustBees, SandSpiderAI, SandWormAI, SpringManAI, TestEnemy]"));
	}
}
