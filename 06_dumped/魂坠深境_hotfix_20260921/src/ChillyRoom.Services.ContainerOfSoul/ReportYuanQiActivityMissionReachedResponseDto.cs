using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ReportYuanQiActivityMissionReachedResponseDto
{
	[JsonProperty("state", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public YuanQiActivityStateDto State { get; set; }
}
