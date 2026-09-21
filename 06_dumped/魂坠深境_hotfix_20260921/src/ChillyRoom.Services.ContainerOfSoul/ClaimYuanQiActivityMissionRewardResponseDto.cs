using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClaimYuanQiActivityMissionRewardResponseDto
{
	[JsonProperty("state", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public YuanQiActivityStateDto State { get; set; }

	[JsonProperty("rewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<GlobalRewardInfo> Rewards { get; set; } = new List<GlobalRewardInfo>();

	[JsonProperty("isDuplicateClaim", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool IsDuplicateClaim { get; set; }
}
