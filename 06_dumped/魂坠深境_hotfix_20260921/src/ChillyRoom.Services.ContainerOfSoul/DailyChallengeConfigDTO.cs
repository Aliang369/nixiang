using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class DailyChallengeConfigDTO
{
	[JsonProperty("challengeId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ChallengeId { get; set; }

	[JsonProperty("ruleContractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string RuleContractId { get; set; }

	[JsonProperty("scoreContractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ScoreContractId { get; set; }

	[JsonProperty("endTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime EndTime { get; set; }

	[JsonProperty("completionRewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<DailyChallengeRewardDisplay> CompletionRewards { get; set; }

	[JsonProperty("weeklyClaimLimit", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int WeeklyClaimLimit { get; set; }
}
