using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class YuanQiActivityStateDto
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string ActivityId { get; set; }

	[JsonProperty("isOpen", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool IsOpen { get; set; }

	[JsonProperty("activityEndTimeUtc", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ActivityEndTimeUtc { get; set; }

	[JsonProperty("missions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<YuanQiActivityMissionStateDto> Missions { get; set; } = new List<YuanQiActivityMissionStateDto>();
}
