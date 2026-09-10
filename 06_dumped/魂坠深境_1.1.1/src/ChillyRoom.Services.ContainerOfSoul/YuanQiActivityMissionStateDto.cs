using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class YuanQiActivityMissionStateDto
{
	[JsonProperty("missionId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string MissionId { get; set; }

	[JsonProperty("progress", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int Progress { get; set; }

	[JsonProperty("status", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int Status { get; set; }
}
