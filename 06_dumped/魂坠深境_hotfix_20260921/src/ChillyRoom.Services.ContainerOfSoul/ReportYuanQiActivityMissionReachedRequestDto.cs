using System;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ReportYuanQiActivityMissionReachedRequestDto
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string ActivityId { get; set; }

	[JsonProperty("missionId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string MissionId { get; set; }

	[JsonProperty("evidenceType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string EvidenceType { get; set; }

	[JsonProperty("progress", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int Progress { get; set; }

	[JsonProperty("clientEventTimeUtc", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public DateTime ClientEventTimeUtc { get; set; }

	[JsonProperty("gameGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string GameGuid { get; set; }

	[JsonProperty("characterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string CharacterId { get; set; }
}
