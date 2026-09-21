using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GameRecordBrief
{
	[JsonProperty("GameRecordGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string GameRecordGuid { get; set; }

	[JsonProperty("PlayerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long PlayerId { get; set; }

	[JsonProperty("PlayerNickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PlayerNickName { get; set; }

	[JsonProperty("CharacterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CharacterId { get; set; }

	[JsonProperty("Success", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Success { get; set; }

	[JsonProperty("ContractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ContractId { get; set; }

	[JsonProperty("DifficultyLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel DifficultyLevel { get; set; }

	[JsonProperty("Progress", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Progress { get; set; }

	[JsonProperty("Duration", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public TimeSpan Duration { get; set; }

	[JsonProperty("rankScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RankScore { get; set; }

	[JsonProperty("pvpChanllengeGameRecord", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public PVPChanllengeGameRecord PvpChanllengeGameRecord { get; set; }

	[JsonProperty("RecordTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime RecordTime { get; set; }

	[JsonProperty("finsihDateTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime FinsihDateTime { get; set; }
}
