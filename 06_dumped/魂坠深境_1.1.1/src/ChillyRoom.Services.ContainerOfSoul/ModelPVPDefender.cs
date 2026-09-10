using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ModelPVPDefender
{
	[JsonProperty("id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Guid Id { get; set; }

	[JsonProperty("sourcePlayerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long SourcePlayerId { get; set; }

	[JsonProperty("sourcePlayerNickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string SourcePlayerNickName { get; set; }

	[JsonProperty("defenderName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string DefenderName { get; set; }

	[JsonProperty("avatarId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string AvatarId { get; set; }

	[JsonProperty("remainGetBonusMatterCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RemainGetBonusMatterCount { get; set; }

	[JsonProperty("deckPreset", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public PVPDefenderDeckPreset DeckPreset { get; set; }

	[JsonProperty("battleRecords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<PVPDefenderBattleRecord> BattleRecords { get; set; }

	[JsonProperty("creationTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime CreationTime { get; set; }
}
