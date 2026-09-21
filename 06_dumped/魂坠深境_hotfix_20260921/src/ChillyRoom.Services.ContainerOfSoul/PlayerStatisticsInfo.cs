using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PlayerStatisticsInfo
{
	[JsonProperty("castItemCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long CastItemCount { get; set; }

	[JsonProperty("coinGetCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long CoinGetCount { get; set; }

	[JsonProperty("crystalGetCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long CrystalGetCount { get; set; }

	[JsonProperty("damageCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long DamageCount { get; set; }

	[JsonProperty("defeatEnemyCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DefeatEnemyCount { get; set; }

	[JsonProperty("encounterEventCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int EncounterEventCount { get; set; }

	[JsonProperty("gameCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int GameCount { get; set; }

	[JsonProperty("winGameCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int WinGameCount { get; set; }

	[JsonProperty("highestFinishedDifficulty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel HighestFinishedDifficulty { get; set; }

	[JsonProperty("currentWinStreak", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CurrentWinStreak { get; set; }

	[JsonProperty("maxWinStreak", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int MaxWinStreak { get; set; }

	[JsonProperty("difficultyStatistics", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<int, GameModeStatisticsInfo> DifficultyStatistics { get; set; }

	[JsonProperty("classStatistics", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, PlayerClassStatisticsInfo> ClassStatistics { get; set; }
}
