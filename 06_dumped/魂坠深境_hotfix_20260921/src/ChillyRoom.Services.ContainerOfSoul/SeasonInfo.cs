using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class SeasonInfo
{
	[JsonProperty("seasonID", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string SeasonID { get; set; }

	[JsonProperty("seasonLevelLineInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public LevelLineInfo SeasonLevelLineInfo { get; set; }

	[JsonProperty("seasonPassUnlocked", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool SeasonPassUnlocked { get; set; }

	[JsonProperty("highSeasonPassUnlocked", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool HighSeasonPassUnlocked { get; set; }
}
