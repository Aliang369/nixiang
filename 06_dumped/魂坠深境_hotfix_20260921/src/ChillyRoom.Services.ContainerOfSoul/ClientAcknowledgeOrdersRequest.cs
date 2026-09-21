using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientAcknowledgeOrdersRequest
{
	[JsonProperty("orderIds")]
	public List<long> OrderIds { get; set; } = new List<long>();
}
