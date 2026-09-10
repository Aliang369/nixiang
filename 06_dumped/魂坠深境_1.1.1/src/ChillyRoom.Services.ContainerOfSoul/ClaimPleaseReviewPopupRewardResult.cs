using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public class ClaimPleaseReviewPopupRewardResult
{
	[JsonProperty("rewardInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public List<GlobalRewardInfo> RewardInfos { get; set; }

	public GlobalRewardPackage GiveRewards(ApplicationStateInfo asi)
	{
		if (RewardInfos.IsNullOrEmpty())
		{
			return null;
		}
		NetworkConvertContext context = new NetworkConvertContext(asi);
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(RewardInfos.Select((GlobalRewardInfo r) => r.ToClient(context)).ToList());
		globalRewardPackage.GiveAll(asi);
		return globalRewardPackage;
	}
}
