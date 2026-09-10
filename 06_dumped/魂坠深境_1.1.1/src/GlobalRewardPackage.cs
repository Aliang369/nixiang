using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GlobalRewardPackage
{
	public class ObsessionConvertInfo
	{
		public GlobalRewardInfo fromReward;

		public GlobalRewardInfo toReward;

		public int tokenCount;
	}

	public string title;

	public string hint;

	public List<GlobalRewardInfo> rewards;

	public List<ObsessionConvertInfo> obsessionConvertInfos;

	public GlobalRewardInfo.RewardNestedType nestedType;

	public int SumTokenCompensate => rewards?.Sum((GlobalRewardInfo r) => r.SumTokenCompensate) ?? 0;

	public int SumItemSkinTokenCompensate => rewards?.Sum((GlobalRewardInfo r) => r.SumItemSkinTokenCompensate) ?? 0;

	public int SumTokenSoul => rewards?.Sum((GlobalRewardInfo r) => (r?.giveAppendInfo?.soul).GetValueOrDefault()) ?? 0;

	public bool WithAnyRepeatReward => rewards?.Any((GlobalRewardInfo r) => r.giveAppendInfo?.repeated ?? false) ?? false;

	public QualityLevel qualityLevel
	{
		get
		{
			if (!rewards.IsNullOrEmpty())
			{
				return rewards.Max((GlobalRewardInfo r) => r.GlobalItemQuality);
			}
			return QualityLevel.None;
		}
	}

	public bool anyUnhiddenReward => rewards?.Any((GlobalRewardInfo r) => !r.isHidden) ?? false;

	public bool IsObsessionConvertPackage => !obsessionConvertInfos.IsNullOrEmpty();

	public GlobalRewardPackage()
	{
	}

	public GlobalRewardPackage(List<GlobalRewardInfo> rewards)
	{
		this.rewards = rewards;
	}

	public GlobalRewardPackage(GlobalRewardInfo reward)
	{
		if (!reward.nestedRewards.IsEmptyOrNull())
		{
			rewards = reward.nestedRewards;
			nestedType = reward.nestedType;
		}
		else
		{
			rewards = new List<GlobalRewardInfo> { reward };
		}
	}

	public void GiveAll(ApplicationStateInfo applicationState)
	{
		foreach (GlobalRewardInfo reward in rewards)
		{
			reward.Give(applicationState);
		}
	}

	public static GlobalRewardPackage CreateObsessionConvertPackage(IEnumerable<GlobalRewardInfo> rewards)
	{
		List<GlobalRewardInfo> source = rewards?.Where((GlobalRewardInfo r) => r != null && r.giveAppendInfo?.convertObsessionToDebris == true && r.matter?.matterDef != null && r.matter.count > 0 && r.refCharacter != null).ToList();
		if (source.IsNullOrEmpty())
		{
			return null;
		}
		List<ObsessionConvertInfo> source2 = source.Select(CreateConvertInfo).ToList();
		return new GlobalRewardPackage(source2.Select((ObsessionConvertInfo i) => i.toReward).ToList())
		{
			title = ScriptableObjectSingleton<TextDataPreset>.instance.term_repeat_obsession,
			hint = ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession_turn,
			obsessionConvertInfos = source2
		};
		static ObsessionConvertInfo CreateConvertInfo(GlobalRewardInfo reward)
		{
			PlayerCharacterDef refCharacter = reward.refCharacter;
			GlobalRewardInfo fromReward = new GlobalRewardInfo
			{
				matter = new MatterInfo(refCharacter.obsession, 1),
				giveAppendInfo = new GlobalRewardGiveAppendInfo
				{
					repeated = true,
					isNewGive = false
				}
			};
			return new ObsessionConvertInfo
			{
				fromReward = fromReward,
				toReward = new GlobalRewardInfo
				{
					matter = new MatterInfo(reward.matter.matterDef, reward.matter.count)
				},
				tokenCount = reward.SumTokenCompensate
			};
		}
	}

	public List<GlobalRewardPackage> SeparateRewardPackageByNestedType()
	{
		List<GlobalRewardPackage> list = new List<GlobalRewardPackage>();
		List<GlobalRewardInfo> list2 = new List<GlobalRewardInfo>();
		List<GlobalRewardPackage> list3 = new List<GlobalRewardPackage>();
		GlobalRewardInfo.RewardNestedType rewardNestedType = GlobalRewardInfo.RewardNestedType.None;
		foreach (GlobalRewardInfo reward in rewards)
		{
			if (reward.nestedType == GlobalRewardInfo.RewardNestedType.None)
			{
				list2.Add(reward);
				continue;
			}
			if (reward.nestedType == GlobalRewardInfo.RewardNestedType.RandomChest)
			{
				rewardNestedType = GlobalRewardInfo.RewardNestedType.RandomChest;
				list2.AddRange(reward.nestedRewards);
				continue;
			}
			GlobalRewardPackage item = new GlobalRewardPackage
			{
				title = title,
				hint = hint,
				rewards = reward.nestedRewards,
				nestedType = reward.nestedType
			};
			list3.Add(item);
		}
		if (list2.Count > 0)
		{
			GlobalRewardPackage item2 = new GlobalRewardPackage
			{
				title = title,
				hint = hint,
				rewards = list2,
				nestedType = rewardNestedType
			};
			list.Add(item2);
		}
		list.AddRange(list3);
		return list;
	}

	public override string ToString()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GlobalRewardInfo reward in rewards)
			{
				stringBuilder.Append(((IGlobalItemDisplaySetup)reward)?.GlobalItemTitle);
			}
			return stringBuilder.ToString();
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[GlobalRewardPackage] - ToString() - {arg}");
			return "[error]";
		}
	}
}
