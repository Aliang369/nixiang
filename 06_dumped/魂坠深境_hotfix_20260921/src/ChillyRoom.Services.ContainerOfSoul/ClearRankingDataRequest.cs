using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ClearRankingDataRequest
{
	[JsonProperty("playerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long PlayerId { get; set; }

	[JsonProperty("rankingKey", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string RankingKey { get; set; }

	[JsonProperty("onlineAreaId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string OnlineAreaId { get; set; }
}
