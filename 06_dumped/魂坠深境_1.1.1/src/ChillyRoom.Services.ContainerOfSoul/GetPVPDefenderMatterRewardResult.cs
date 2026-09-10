using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetPVPDefenderMatterRewardResult
{
	[JsonProperty("rewardMatterInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public MatterInfo RewardMatterInfo { get; set; }

	public GlobalRewardPackage GiveReward(ApplicationStateInfo asi)
	{
		NetworkConvertContext context = new NetworkConvertContext(asi);
		global::MatterInfo matter = RewardMatterInfo.ToClient(context);
		global::GlobalRewardInfo globalRewardInfo = new global::GlobalRewardInfo();
		globalRewardInfo.matter = matter;
		globalRewardInfo.rewardSourceType = GlobalRewardSourceType.PVPGetBonus;
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(new List<global::GlobalRewardInfo> { globalRewardInfo });
		globalRewardPackage.GiveAll(asi);
		return globalRewardPackage;
	}
}
