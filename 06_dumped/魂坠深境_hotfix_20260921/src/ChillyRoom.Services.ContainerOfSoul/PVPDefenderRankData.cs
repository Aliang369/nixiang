using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PVPDefenderRankData
{
	[JsonProperty("index", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Index { get; set; }

	[JsonProperty("defenderGuid", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Guid DefenderGuid { get; set; }

	[JsonProperty("defenderNickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string DefenderNickName { get; set; }

	[JsonProperty("defenderAvatarId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string DefenderAvatarId { get; set; }

	[JsonProperty("ownerPlayerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long OwnerPlayerId { get; set; }

	[JsonProperty("ownerPlayerNickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string OwnerPlayerNickName { get; set; }

	[JsonProperty("rankScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RankScore { get; set; }
}
