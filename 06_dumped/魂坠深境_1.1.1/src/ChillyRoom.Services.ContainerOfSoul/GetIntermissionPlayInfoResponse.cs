using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class GetIntermissionPlayInfoResponse
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string ActivityId { get; set; }

	[JsonProperty("isOpen", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool IsOpen { get; set; }

	[JsonProperty("closeReason", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string CloseReason { get; set; }

	[JsonProperty("serverNowUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ServerNowUtc { get; set; }

	[JsonProperty("activityEndTimeUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ActivityEndTimeUtc { get; set; }

	[JsonProperty("currentDayIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int CurrentDayIndex { get; set; }

	[JsonProperty("nextRefreshTimeUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset NextRefreshTimeUtc { get; set; }

	[JsonProperty("paidGiftProgress", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int PaidGiftProgress { get; set; }

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

	[JsonProperty("carryTicketCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int CarryTicketCount { get; set; }

	[JsonProperty("otherworldTicketICount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int OtherworldTicketICount { get; set; }

	[JsonProperty("otherworldGiftCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int OtherworldGiftCount { get; set; }

	[JsonProperty("title", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> Title { get; set; } = new Dictionary<string, string>();

	[JsonProperty("description", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> Description { get; set; } = new Dictionary<string, string>();

	[JsonProperty("helpText", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> HelpText { get; set; } = new Dictionary<string, string>();

	[JsonProperty("rewardSlots", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<IntermissionPlayRewardSlotDto> RewardSlots { get; set; } = new List<IntermissionPlayRewardSlotDto>();

	[JsonProperty("openedCards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<IntermissionPlayOpenedCardDto> OpenedCards { get; set; } = new List<IntermissionPlayOpenedCardDto>();
}
