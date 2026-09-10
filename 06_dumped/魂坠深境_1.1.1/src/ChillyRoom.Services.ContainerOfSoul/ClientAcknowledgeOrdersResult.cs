using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientAcknowledgeOrdersResult
{
	[JsonProperty("acknowledgedOrderIds")]
	public List<long> AcknowledgedOrderIds { get; set; } = new List<long>();
}
