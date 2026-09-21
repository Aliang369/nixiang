using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClaimYuanQiActivityMissionRewardRequestDto
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string ActivityId { get; set; }

	[JsonProperty("missionId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string MissionId { get; set; }
}
