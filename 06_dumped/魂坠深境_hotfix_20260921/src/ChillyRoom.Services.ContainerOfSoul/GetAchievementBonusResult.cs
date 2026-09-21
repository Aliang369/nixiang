using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetAchievementBonusResult
{
	[JsonProperty("bonuses", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<SingleAchievementBonus> Bonuses { get; set; }

	public GlobalRewardPackage GiveAchievementRewards(ApplicationStateInfo asi)
	{
		List<global::GlobalRewardInfo> list = new List<global::GlobalRewardInfo>();
		string text = "";
		foreach (SingleAchievementBonus bonuse in Bonuses)
		{
			NetworkConvertContext context = new NetworkConvertContext(asi);
			AchievementDef achDef = context.FullIdToCTAsset<AchievementDef>(bonuse.Id);
			if (achDef == null)
			{
				continue;
			}
			Achievement achievement = asi.achievementManager.globalAchievements.FirstOrDefault((Achievement a) => a.achievementDef == achDef);
			if (achievement != null)
			{
				text += achDef.achievementName;
				achievement.rewardReceived = true;
				List<global::GlobalRewardInfo> collection = bonuse.Rewards.Select((GlobalRewardInfo r) => r.ToClient(context)).ToList();
				list.AddRange(collection);
			}
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
		globalRewardPackage.title = text;
		globalRewardPackage.GiveAll(asi);
		return globalRewardPackage;
	}
}
