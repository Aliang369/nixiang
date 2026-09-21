using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayDrawResponse
{
	[JsonProperty("requestId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public Guid RequestId { get; set; }

	[JsonProperty("cardIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int CardIndex { get; set; }

	[JsonProperty("slotId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int SlotId { get; set; }

	[JsonProperty("ticketType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public IntermissionPlayTicketType TicketType { get; set; }

	[JsonProperty("rewardView", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public GlobalRewardInfo RewardView { get; set; } = new GlobalRewardInfo();

	[JsonProperty("dailyTicketCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int DailyTicketCount { get; set; }

	[JsonProperty("carryTicketCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int CarryTicketCount { get; set; }

	[JsonProperty("remainingSlotCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int RemainingSlotCount { get; set; }

	[JsonProperty("drawnSlotIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<int> DrawnSlotIds { get; set; } = new List<int>();

	[JsonProperty("openedCards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<IntermissionPlayOpenedCardDto> OpenedCards { get; set; } = new List<IntermissionPlayOpenedCardDto>();

	[JsonProperty("nextRefreshTimeUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset NextRefreshTimeUtc { get; set; }
}
