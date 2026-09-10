using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientGachaHistorySyncRequest
{
	[JsonProperty("gachaPoolId")]
	public string GachaPoolId { get; set; }

	[JsonProperty("gachaPoolIds")]
	public List<string> GachaPoolIds { get; set; }
}
