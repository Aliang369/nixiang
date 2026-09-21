using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class DailyChallengeRankEntryDTO
{
	[JsonProperty("rank", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Rank { get; set; }

	[JsonProperty("playerName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PlayerName { get; set; }

	[JsonProperty("score", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Score { get; set; }

	[JsonProperty("playerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long PlayerId { get; set; }

	[JsonProperty("bestGameRecordId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string BestGameRecordId { get; set; }

	[JsonProperty("characterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CharacterId { get; set; }

	[JsonProperty("difficulty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Difficulty { get; set; }

	[JsonProperty("firstBestSubmittedAt", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime? FirstBestSubmittedAt { get; set; }
}
