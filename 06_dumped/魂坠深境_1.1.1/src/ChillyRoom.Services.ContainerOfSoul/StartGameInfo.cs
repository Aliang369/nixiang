using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class StartGameInfo
{
	[JsonProperty("gameGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string GameGuid { get; set; }

	[JsonProperty("gameRandomSeed", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int GameRandomSeed { get; set; }

	[JsonProperty("challengeId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ChallengeId { get; set; }

	[JsonProperty("challengeRuleContractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ChallengeRuleContractId { get; set; }

	[JsonProperty("challengeEndTime", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime? ChallengeEndTime { get; set; }

	[JsonProperty("foreRunnerLegacyThisGame", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ForerunnerLegacy> ForeRunnerLegacyThisGame { get; set; }

	[JsonProperty("pvpDefenderThisGame", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ModelPVPDefender> PvpDefenderThisGame { get; set; }

	[JsonProperty("bossBlessWeightSnapshot", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public BossBlessWeightSnapshot BossBlessWeightSnapshot { get; set; }
}
