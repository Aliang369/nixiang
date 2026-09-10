using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetRoutineQuestBonusResult
{
	[JsonProperty("bonuss", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<SingleRoutineQuestBonus> Bonuss { get; set; }

	public GlobalRewardPackage GiveRoutineQuestRewards(ApplicationStateInfo asi)
	{
		List<global::GlobalRewardInfo> list = new List<global::GlobalRewardInfo>();
		NetworkConvertContext context = new NetworkConvertContext(asi);
		foreach (SingleRoutineQuestBonus item in Bonuss)
		{
			asi.routineQuestManager.GetQuestState(item.GuestGuid).gotRewardLayer++;
			list.AddRange(item.Rewards.Select((GlobalRewardInfo r) => r.ToClient(context)).ToList());
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
		globalRewardPackage.GiveAll(asi);
		return globalRewardPackage;
	}
}
