using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayReclaimTicketsRequest
{
	[JsonProperty("requestId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public Guid RequestId { get; set; }

	[JsonProperty("ticketType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public IntermissionPlayTicketType TicketType { get; set; }

	[JsonProperty("count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int Count { get; set; }
}
