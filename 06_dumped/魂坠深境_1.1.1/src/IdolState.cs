using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class IdolState
{
	[SaveField(null, null)]
	public IdolDef idolDef;

	[SaveField(null, null)]
	public int currentLevel;

	[SaveField(null, null)]
	public DifficultyLevel currentDifficultyLevel;

	[SaveField(null, null)]
	public DifficultyLevel mainlineDifficulty;

	[SaveField(null, null)]
	public int seasonExperience;

	[SaveField(null, null)]
	public int seasonSoul;

	[SaveField(null, null)]
	public Season season;

	public CharacterClassDef characterClassDef => Bootstrap.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets.Find((CharacterClassDef c) => c.mClass == idolDef.attachClass);

	public MatterInfo soul => Bootstrap.applicationState.matterManager.GetMatterInfo(soulDef);

	public MatterDef soulDef => characterClassDef.soulMatter;

	public MatterInfo upgradeCost => new MatterInfo(soulDef, 1);

	public IdolState()
	{
	}

	public IdolState(IdolDef idolDef)
	{
		this.idolDef = idolDef;
		currentLevel = 0;
		currentDifficultyLevel = DifficultyLevel.Not;
		mainlineDifficulty = DifficultyLevel.Not;
		seasonExperience = 0;
		season = Bootstrap.applicationState.seasonManager.currentSeason;
	}

	public bool CanOffer()
	{
		if (GetUnofferedSoulSacrifice() > 0)
		{
			return currentLevel < idolDef.maxLevel;
		}
		return false;
	}

	public int GetUnofferedSoulSacrifice()
	{
		return -1;
	}

	public int GetMaxOfferLevel()
	{
		int unofferedSoulSacrifice = GetUnofferedSoulSacrifice();
		return Mathf.Min(currentLevel + unofferedSoulSacrifice, idolDef.maxLevel);
	}

	public int GetRequiredSoulSacrificeForLevel(int targetLevel)
	{
		return targetLevel - currentLevel;
	}

	public bool CanUpgrade(ApplicationStateInfo applicationState)
	{
		if (currentLevel >= idolDef.maxLevel)
		{
			return false;
		}
		if (idolDef.GetLevel(currentLevel + 1) == null)
		{
			return false;
		}
		if (!applicationState.matterManager.HasEnoughMatter(upgradeCost))
		{
			return false;
		}
		return true;
	}

	public void Upgrade(ApplicationStateInfo applicationState, Action<GlobalRewardPackage> onComplete = null)
	{
		if (!CanUpgrade(applicationState) || !GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		GeneralGameAsyncOperation<UpgradeIdolRequest, UpgradeIdolResult> op = new GeneralGameAsyncOperation<UpgradeIdolRequest, UpgradeIdolResult>(new UpgradeIdolRequest
		{
			IdolId = idolDef.FullID
		}, applicationState.ChillyRoomSDK.Character.UpgradeIdolAsync);
		op.OnComplete(delegate
		{
			NetworkConvertContext context = new NetworkConvertContext(applicationState);
			List<GlobalRewardInfo> list = op.OpResult.Reward.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(context)).ToList();
			foreach (GlobalRewardInfo item in list)
			{
				if (item.forgetShinePoint)
				{
					item.characterClassDef = characterClassDef;
				}
			}
			OnComplete(list);
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_quest_reward);
		void OnComplete(List<GlobalRewardInfo> reward)
		{
			applicationState.matterManager.ConsumeMatter(upgradeCost, "神像升级");
			if (idolDef.GetLevel(currentLevel + 1) != null)
			{
				CharacterClassDef characterClassDef = GetCharacterClassDef();
				foreach (GlobalRewardInfo item2 in reward)
				{
					if (item2 != null && item2.forgetShinePoint)
					{
						item2.characterClassDef = characterClassDef;
					}
				}
				GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(reward);
				globalRewardPackage.GiveAll(applicationState);
				currentLevel++;
				ThinkingAnalyticsExtends.TrackIdolOfferSoul(applicationState, GetCharacterClassDef(), currentLevel);
				onComplete?.Invoke(globalRewardPackage);
				applicationState.eventManager.idolUpgradeCompleteEvent.TriggerEvent(this);
			}
		}
	}

	public bool IsMaxLevel()
	{
		return idolDef.maxLevel == currentLevel;
	}

	public int CharacterSoulCount(PlayerCharacterDef characterDef)
	{
		PlayerCharacterState state = characterDef.GetState(Bootstrap.applicationState);
		if (state.locked)
		{
			return 0;
		}
		return ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.newCharacterReward + state.obsessionLevel;
	}

	public CharacterClassDef GetCharacterClassDef()
	{
		return Bootstrap.applicationState.assetCatalog.characterClassDefCatalog.ValidAssets.Find((CharacterClassDef c) => c.mClass == idolDef.attachClass);
	}

	public CharacterClassState GetCharacterClassState()
	{
		return Bootstrap.applicationState.characterManager.GetCharacterClassState(GetCharacterClassDef());
	}

	public int GetForgetShrinePoint()
	{
		int num = 0;
		for (int i = 0; i < currentLevel && i < idolDef.levels.Count; i++)
		{
			foreach (GlobalRewardSetup item in idolDef.levels[i].reward)
			{
				if (item != null && item is ForgetShrinePointReward)
				{
					num++;
				}
			}
		}
		return num;
	}
}
