using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GameOverResult
{
	[JsonProperty("alreadySettled", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool AlreadySettled { get; set; }

	[JsonProperty("levelLineChangeExpResults", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ChangeExpResult> LevelLineChangeExpResults { get; set; }

	[JsonProperty("gameOverContractRewardInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GameOverContractRewardInfo GameOverContractRewardInfo { get; set; }

	[JsonProperty("rankScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RankScore { get; set; }

	[JsonProperty("expSum", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ExpSum { get; set; }

	[JsonProperty("commonCurrency", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CommonCurrency { get; set; }

	[JsonProperty("gems", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, int> Gems { get; set; }

	[JsonProperty("expChangeRecords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ExpChangeRecord> ExpChangeRecords { get; set; }

	[JsonProperty("inGameMatterRewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<MatterInfo> InGameMatterRewards { get; set; }

	[JsonProperty("dailyChallengeSettlement", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DailyChallengeSettlement DailyChallengeSettlement { get; set; }
}
