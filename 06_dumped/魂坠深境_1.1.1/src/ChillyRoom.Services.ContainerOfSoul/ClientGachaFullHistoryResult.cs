using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientGachaFullHistoryResult
{
	[JsonProperty("items")]
	public List<GachaDrawHistory> Items { get; set; } = new List<GachaDrawHistory>();

	[JsonProperty("totalDrawCount")]
	public int TotalDrawCount { get; set; }
}
