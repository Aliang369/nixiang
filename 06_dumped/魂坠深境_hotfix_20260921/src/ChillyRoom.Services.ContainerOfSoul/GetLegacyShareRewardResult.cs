using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetLegacyShareRewardResult
{
	[JsonProperty("reward", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GlobalRewardInfo Reward { get; set; }

	[JsonProperty("rewardUpvoteCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RewardUpvoteCount { get; set; }

	public GlobalRewardPackage TryGiveRewards(ApplicationStateInfo asi)
	{
		if (Reward == null)
		{
			return null;
		}
		NetworkConvertContext context = new NetworkConvertContext(asi);
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(Reward.ToClient(context));
		globalRewardPackage.title = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_legacy_share_upvote_reward_from_arg0_upvote.FormatWith(RewardUpvoteCount);
		globalRewardPackage.GiveAll(asi);
		return globalRewardPackage;
	}
}
