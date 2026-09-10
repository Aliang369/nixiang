using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientGachaHistoryQueryRequest
{
	[JsonProperty("pageIndex")]
	public int PageIndex { get; set; }

	[JsonProperty("pageSize")]
	public int PageSize { get; set; }

	[JsonProperty("gachaPoolId")]
	public string GachaPoolId { get; set; }

	[JsonProperty("gachaPoolIds")]
	public List<string> GachaPoolIds { get; set; }
}
