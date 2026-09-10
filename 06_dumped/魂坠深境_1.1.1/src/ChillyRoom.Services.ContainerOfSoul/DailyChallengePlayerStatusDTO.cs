using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class DailyChallengePlayerStatusDTO
{
	[JsonProperty("hasChallenged", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool HasChallenged { get; set; }

	[JsonProperty("score", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Score { get; set; }

	[JsonProperty("rank", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? Rank { get; set; }

	[JsonProperty("claimStatus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ClaimStatus { get; set; }

	[JsonProperty("weeklyRemainingClaims", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int WeeklyRemainingClaims { get; set; }

	[JsonProperty("bestEntry", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DailyChallengeRankEntryDTO BestEntry { get; set; }
}
