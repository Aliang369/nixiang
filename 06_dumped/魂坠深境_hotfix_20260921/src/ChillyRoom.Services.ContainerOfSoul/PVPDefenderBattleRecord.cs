using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PVPDefenderBattleRecord
{
	[JsonProperty("pvpDefenderGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PvpDefenderGuid { get; set; }

	[JsonProperty("gameRecordGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string GameRecordGuid { get; set; }

	[JsonProperty("dealtDamageToChallenger", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DealtDamageToChallenger { get; set; }

	[JsonProperty("challengerBattleFinishType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public BattleFinishType ChallengerBattleFinishType { get; set; }

	[JsonProperty("challengerPlayerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long ChallengerPlayerId { get; set; }

	[JsonProperty("challengerPlayerNickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ChallengerPlayerNickName { get; set; }

	[JsonProperty("defenderName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string DefenderName { get; set; }

	[JsonProperty("defenderAvatarId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string DefenderAvatarId { get; set; }

	[JsonProperty("defenderSourcePlayerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long DefenderSourcePlayerId { get; set; }

	[JsonProperty("defenderSourcePlayerNickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string DefenderSourcePlayerNickName { get; set; }

	[JsonProperty("battleEndTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime BattleEndTime { get; set; }
}
