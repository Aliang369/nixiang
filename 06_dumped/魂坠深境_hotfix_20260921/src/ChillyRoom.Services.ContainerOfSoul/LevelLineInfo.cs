using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class LevelLineInfo
{
	[JsonProperty("levelLineID", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string LevelLineID { get; set; }

	[JsonProperty("levelIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int LevelIndex { get; set; }

	[JsonProperty("exp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Exp { get; set; }

	[JsonProperty("dumpExtraLevelCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DumpExtraLevelCount { get; set; }

	[JsonProperty("nextDumpRewardIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int NextDumpRewardIndex { get; set; }

	[JsonProperty("baseRewardGotLevelIndex", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<int> BaseRewardGotLevelIndex { get; set; }

	[JsonProperty("appendRewardGotLevelIndex", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<int> AppendRewardGotLevelIndex { get; set; }
}
