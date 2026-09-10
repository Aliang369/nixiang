using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayClaimDailyTaskResponse
{
	[JsonProperty("dailyTaskCompleted", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool DailyTaskCompleted { get; set; }

	[JsonProperty("dailyTaskCompletedCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int DailyTaskCompletedCount { get; set; }

	[JsonProperty("dailyTicketGranted", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool DailyTicketGranted { get; set; }

	[JsonProperty("dailyTicketGrantedCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int DailyTicketGrantedCount { get; set; }

	[JsonProperty("dailyTicketCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int DailyTicketCount { get; set; }

	[JsonProperty("reward", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public GlobalRewardInfo Reward { get; set; } = new GlobalRewardInfo();
}
