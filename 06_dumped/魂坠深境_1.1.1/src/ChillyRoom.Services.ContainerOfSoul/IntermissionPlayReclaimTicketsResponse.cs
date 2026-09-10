using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayReclaimTicketsResponse
{
	[JsonProperty("requestId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public Guid RequestId { get; set; }

	[JsonProperty("ticketType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public IntermissionPlayTicketType TicketType { get; set; }

	[JsonProperty("reclaimedCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int ReclaimedCount { get; set; }

	[JsonProperty("giftCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int GiftCount { get; set; }

	[JsonProperty("otherworldTicketICount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int OtherworldTicketICount { get; set; }

	[JsonProperty("otherworldTicketIICount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int OtherworldTicketIICount { get; set; }

	[JsonProperty("otherworldGiftCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int OtherworldGiftCount { get; set; }
}
