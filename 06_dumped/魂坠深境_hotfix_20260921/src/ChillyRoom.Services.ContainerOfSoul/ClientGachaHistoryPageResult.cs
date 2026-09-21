using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientGachaHistoryPageResult
{
	[JsonProperty("items")]
	public List<GachaDrawHistory> Items { get; set; } = new List<GachaDrawHistory>();

	[JsonProperty("totalCount")]
	public int TotalCount { get; set; }

	[JsonProperty("pageIndex")]
	public int PageIndex { get; set; }

	[JsonProperty("pageSize")]
	public int PageSize { get; set; }

	[JsonProperty("hasMore")]
	public bool HasMore { get; set; }
}
