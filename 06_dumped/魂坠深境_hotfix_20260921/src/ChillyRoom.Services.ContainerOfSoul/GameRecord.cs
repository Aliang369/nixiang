using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GameRecord
{
	[JsonProperty("gameGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string GameGuid { get; set; }

	[JsonProperty("playerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long PlayerId { get; set; }

	[JsonProperty("nickname", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Nickname { get; set; }

	[JsonProperty("characterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CharacterId { get; set; }

	[JsonProperty("contractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ContractId { get; set; }

	[JsonProperty("success", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Success { get; set; }

	[JsonProperty("finalProgress", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int FinalProgress { get; set; }

	[JsonProperty("finalStage", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int FinalStage { get; set; }

	[JsonProperty("difficulty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel Difficulty { get; set; }

	[JsonProperty("duration", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public TimeSpan Duration { get; set; }

	[JsonProperty("recordTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime RecordTime { get; set; }

	[JsonProperty("expBonus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ExpBonus { get; set; }

	[JsonProperty("expChangeRecords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ExpChangeRecord> ExpChangeRecords { get; set; }

	[JsonProperty("rankScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RankScore { get; set; }

	[JsonProperty("deck", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<CardRecord> Deck { get; set; }

	[JsonProperty("influences", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<InfluenceRecord> Influences { get; set; }

	[JsonProperty("skillRecords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<SkillRecord> SkillRecords { get; set; }

	[JsonProperty("journals", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GameJournalRecord> Journals { get; set; }

	[JsonProperty("discoveredDefIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> DiscoveredDefIds { get; set; }

	[JsonProperty("finishAchievementIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> FinishAchievementIds { get; set; }

	[JsonProperty("enteredNodePoint", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int EnteredNodePoint { get; set; }

	[JsonProperty("inGameMatterRewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<MatterInfo> InGameMatterRewards { get; set; }

	[JsonProperty("mostDamageEnemy", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public AssetValueRecord MostDamageEnemy { get; set; }

	[JsonProperty("mostDamageSources", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<AssetValueRecord> MostDamageSources { get; set; }

	[JsonProperty("pvpChanllengeGameRecord", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public PVPChanllengeGameRecord PvpChanllengeGameRecord { get; set; }

	[JsonProperty("dailyChallengeGameRecord", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DailyChallengeGameRecord DailyChallengeGameRecord { get; set; }

	[JsonProperty("dailyChallengeSettlement", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DailyChallengeSettlement DailyChallengeSettlement { get; set; }

	[JsonProperty("finishDateTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime FinishDateTime { get; set; }

	[JsonProperty("antiCheatRuleFlags", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int AntiCheatRuleFlags { get; set; }

	[JsonProperty("gemConfig", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<int, int> GemConfig { get; set; }

	[JsonProperty("atcTamperCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ATCTamperCount { get; set; }

	[JsonProperty("atcMismatchReadCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ATCMismatchReadCount { get; set; }

	[JsonProperty("bossBlessStatistics", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<BossBlessStatisticItem> BossBlessStatistics { get; set; }
}
