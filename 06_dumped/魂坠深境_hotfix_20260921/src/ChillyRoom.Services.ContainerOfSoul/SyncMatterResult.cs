using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class SyncMatterResult
{
	[JsonProperty("matterInfos")]
	public List<MatterInfo> MatterInfos { get; set; } = new List<MatterInfo>();
}
