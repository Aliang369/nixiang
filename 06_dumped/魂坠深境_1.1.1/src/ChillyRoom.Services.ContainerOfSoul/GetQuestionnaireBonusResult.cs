using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetQuestionnaireBonusResult
{
	[JsonProperty("rewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> RewardInfos { get; set; }

	public GlobalRewardPackage GiveRewards(ApplicationStateInfo asi)
	{
		if (RewardInfos.IsNullOrEmpty())
		{
			return null;
		}
		NetworkConvertContext context = new NetworkConvertContext(asi);
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(RewardInfos.Select((GlobalRewardInfo r) => r.ToClient(context)).ToList());
		globalRewardPackage.title = ScriptableObjectSingleton<TextDataPreset>.instance.term_questionnaire_reward_title;
		globalRewardPackage.GiveAll(asi);
		return globalRewardPackage;
	}
}
