using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class CompleteCharacterTrialResult
{
	[JsonProperty("firstCompleted", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool FirstCompleted { get; set; }

	[JsonProperty("completedCharacterTrialIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> CompletedCharacterTrialIds { get; set; }

	[JsonProperty("rewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> Rewards { get; set; }

	public GlobalRewardPackage GiveRewards(ApplicationStateInfo applicationState)
	{
		List<global::GlobalRewardInfo> list = Rewards?.Select((GlobalRewardInfo reward) => reward.ToClient(new NetworkConvertContext(applicationState))).WhereNotNull().ToList() ?? new List<global::GlobalRewardInfo>();
		if (list.Count == 0)
		{
			return null;
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
		globalRewardPackage.GiveAll(applicationState);
		return globalRewardPackage;
	}
}
