using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayDrawRequest
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string ActivityId { get; set; }

	[JsonProperty("requestId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public Guid RequestId { get; set; }

	[JsonProperty("cardIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int CardIndex { get; set; }
}
