using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GameOverContractRewardInfo
{
	[JsonProperty("contractFinishRewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> ContractFinishRewardInfos { get; set; }

	[JsonProperty("contractDifficultyRewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> ContractDifficultyRewardInfos { get; set; }

	[JsonProperty("contractAccountDifficultyRewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> ContractAccountDifficultyRewardInfos { get; set; }

	[JsonProperty("contractClassDifficultyRewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> ContractClassDifficultyRewardInfos { get; set; }

	[JsonProperty("starMapAllFinishRewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> StarMapAllFinishRewardInfos { get; set; }
}
