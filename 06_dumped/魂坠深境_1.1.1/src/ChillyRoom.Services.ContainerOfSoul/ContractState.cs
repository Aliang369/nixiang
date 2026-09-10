using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ContractState
{
	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Id { get; set; }

	[JsonProperty("finishedThisCycle", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool FinishedThisCycle { get; set; }

	[JsonProperty("finishedMaxDifficultyThisCycle", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel FinishedMaxDifficultyThisCycle { get; set; }

	[JsonProperty("lastFinishTime", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime? LastFinishTime { get; set; }

	[JsonProperty("lastFinishSeason", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string LastFinishSeason { get; set; }
}
