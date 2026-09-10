using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class GetYuanQiActivityStateRequestDto
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string ActivityId { get; set; }
}
