using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ChangeExpResult
{
	[JsonProperty("request", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ChangeExpRequest Request { get; set; }

	[JsonProperty("levelLineId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string LevelLineId { get; set; }

	[JsonProperty("initExp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int InitExp { get; set; }

	[JsonProperty("initLevelIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int InitLevelIndex { get; set; }

	[JsonProperty("finalExp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int FinalExp { get; set; }

	[JsonProperty("finalLevelIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int FinalLevelIndex { get; set; }

	[JsonProperty("nextDumpRewardIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int NextDumpRewardIndex { get; set; }

	[JsonProperty("levelUpInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<LevelUpRewardInfo> LevelUpInfos { get; set; }

	[JsonProperty("dumpRewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> DumpRewards { get; set; }

	[JsonProperty("fromQuest", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool FromQuest { get; set; }
}
