using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class DailyChallengeSettlement
{
	[JsonProperty("challengeId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ChallengeId { get; set; }

	[JsonProperty("status", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Status { get; set; }

	[JsonProperty("reasonCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ReasonCode { get; set; }

	[JsonProperty("challengeCompleted", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool ChallengeCompleted { get; set; }

	[JsonProperty("hasChallenged", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool HasChallenged { get; set; }

	[JsonProperty("scoreContractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ScoreContractId { get; set; }

	[JsonProperty("scoreMetrics", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DailyChallengeScoreMetrics ScoreMetrics { get; set; }

	[JsonProperty("baseExperience", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int BaseExperience { get; set; }

	[JsonProperty("competitionScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CompetitionScore { get; set; }

	[JsonProperty("preMultiplierScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int PreMultiplierScore { get; set; }

	[JsonProperty("bossMultiplier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public double BossMultiplier { get; set; }

	[JsonProperty("difficultyMultiplier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public double DifficultyMultiplier { get; set; }

	[JsonProperty("finalScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int FinalScore { get; set; }

	[JsonProperty("completionRewardGranted", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool CompletionRewardGranted { get; set; }

	[JsonProperty("completionRewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> CompletionRewardInfos { get; set; }
}
