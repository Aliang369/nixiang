using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public class GameOverRewardPanel : UIBase
{
	[SerializeField]
	public CanvasGroup canvasGroup;

	[SerializeField]
	public float cellIntervalTime = 0.5f;

	[SerializeField]
	public GachaResultDisplay gachaResultDisplay;

	[SerializeField]
	public GameOverRewardCellGroup rewardCellGroupPrefab;

	[NonSerialized]
	public bool finished;

	[NonSerialized]
	private GameRecord record;

	[NonSerialized]
	private CellUIPrefabPool<GameOverRewardCellGroup, (string, List<GlobalRewardInfo>, bool)> rewardGroupPool;

	public LevelUpDisplay levelUpDisplay => UIBase.gameState.UIManager.gameOverPanel.levelUpDisplay;

	public MultyLevelUpDisplay multyLevelUpDisplay => UIBase.gameState.UIManager.gameOverPanel.multyLevelUpDisplay;

	protected override void Init()
	{
		InitUI(gachaResultDisplay);
		GetPrefabPool(ref rewardGroupPool, rewardCellGroupPrefab);
	}

	public IEnumerator ShowRewardsCor()
	{
		finished = false;
		yield return new WaitForSeconds(cellIntervalTime);
		UIBase.gameState.UIManager.gameVirtualCamera.cameraFrontFilter.gameObject.SetActive(value: false);
		yield return levelUpDisplay.DisplayCor(record.globalLineChangeExpResult, base.textData.term_global_reward);
		yield return ShowMultiLevelUp();
		yield return ShowClassDifficultyUnlockedHint();
		yield return ShowRewardPackage(new GlobalRewardPackage(record.GetGemRewards()), base.textData.term_gemReward);
		yield return ShowSoulSacrificeRewards();
		rewardGroupPool.DespawnAll();
		Spawn(base.textData.term_global_reward, record.globalLineChangeExpResult?.GetAllUnFoldRewards());
		Spawn(base.textData.term_gemReward, record.GetGemRewards());
		Spawn(base.textData.term_contractReward, record.contractFinishRewards);
		Spawn(base.textData.term_gameReward, record.gameLootResult.Select((MatterInfo m) => new GlobalRewardInfo
		{
			matter = m
		}).ToList());
		if (record.difficultyTrialReward != null)
		{
			Spawn(base.textData.term_difficulty_trial, new List<GlobalRewardInfo> { record.difficultyTrialReward });
		}
		IdolManager.SoulExpInfo soulExpInfo = record.soulExpInfo;
		if (soulExpInfo != null && soulExpInfo.count > 0)
		{
			Spawn(base.textData.term_season_trial, new List<GlobalRewardInfo>
			{
				new GlobalRewardInfo
				{
					rewardSourceType = GlobalRewardSourceType.SoulSacrificeReward,
					matter = record.soulExpInfo.matter
				}
			});
		}
		finished = true;
		void Spawn(string name, List<GlobalRewardInfo> rewardInfos, bool isText = false)
		{
			if (!rewardInfos.IsEmptyOrNull())
			{
				rewardGroupPool.Spawn((name, rewardInfos, isText));
			}
		}
	}

	public void Setup(GameRecord t)
	{
		record = t;
	}

	private IEnumerator ShowRewardPackage(GlobalRewardPackage package, string title)
	{
		package.title = title;
		UIBase.gameState.UIManager.globalRewardInfoDisplay.Setup(package);
		yield return new WaitUntil(() => !UIBase.gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
		if (record.contractFinishRewards != null && record.contractFinishRewards.Count > 0)
		{
			UIBase.gameState.UIManager.globalRewardInfoDisplay.Setup(new GlobalRewardPackage(record.contractFinishRewards)
			{
				title = base.textData.term_reward_of_finished_contract_arg1_with_difficulty_arg0.FormatWith(record.contract.GetDifficultyInfo(UIBase.applicationState).difficultyName, record.contract.contractDef.Name)
			});
			yield return new WaitUntil(() => !UIBase.gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
		}
	}

	public IEnumerator ShowGlobalReward(LevelLine.ChangeExpResult result, string title)
	{
		if (result == null)
		{
			yield break;
		}
		foreach (GlobalRewardInfo allUnFoldReward in result.GetAllUnFoldRewards())
		{
			if (IsUnlockRewardDislplayable(allUnFoldReward))
			{
				GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(allUnFoldReward);
				globalRewardPackage.title = title;
				globalRewardPackage.hint = "";
				UIBase.gameState.UIManager.globalRewardInfoDisplay.Setup(globalRewardPackage);
				yield return new WaitUntil(() => !UIBase.gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
			}
		}
		static bool IsUnlockRewardDislplayable(GlobalRewardInfo reward)
		{
			if (reward.unlockCTSOs.IsNullOrEmpty())
			{
				return false;
			}
			return reward.unlockCTSOs.TrueForAll((CTScriptableObject s) => s is ItemDefinition || s is InfluenceDef || s is RuneDef || s is SkillDef);
		}
	}

	private IEnumerator ShowSoulSacrificeRewards()
	{
		List<GlobalRewardInfo> soulSacrificeRewards = record.soulSacrificeRewards;
		if (soulSacrificeRewards.IsNullOrEmpty())
		{
			yield break;
		}
		IdolManager idolManager = UIBase.gameState.applicationState.idolManager;
		if (idolManager == null)
		{
			yield break;
		}
		if (!idolManager.IsIdolSystemUnlocked())
		{
			GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage();
			globalRewardPackage.title = base.textData.term_soul_sacrifice;
			globalRewardPackage.hint = base.textData.term_idol_system_not_unlocked;
			UIBase.gameState.UIManager.globalRewardInfoDisplay.Setup(globalRewardPackage);
			yield return new WaitUntil(() => !UIBase.gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
		}
		else
		{
			GlobalRewardPackage globalRewardPackage2 = new GlobalRewardPackage();
			globalRewardPackage2.title = base.textData.term_soul_sacrifice ?? "";
			globalRewardPackage2.rewards = soulSacrificeRewards;
			UIBase.gameState.UIManager.globalRewardInfoDisplay.Setup(globalRewardPackage2);
			yield return new WaitUntil(() => !UIBase.gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
		}
	}

	private IEnumerator ShowMultiLevelUp()
	{
		IdolManager idolManager = UIBase.gameState.applicationState.idolManager;
		if (idolManager != null && idolManager.IsIdolSystemUnlocked() && !(multyLevelUpDisplay == null))
		{
			yield return multyLevelUpDisplay.DisplayCor(record);
		}
	}

	private IEnumerator ShowClassDifficultyUnlockedHint()
	{
		GameRecord.DifficultyTrialProgressInfo difficultyTrialProgressInfo = record?.mainlineDifficultyProgressInfo;
		GameRecord gameRecord = record;
		if (gameRecord == null || !gameRecord.success || record.skipUnfinishedNewbieClear || !record.AllowsDifficultyProgression)
		{
			yield break;
		}
		Contract contract = record.contract;
		if (contract == null || contract.contractDef?.contractType != ContractDef.ContractType.BaseGame || record.character?.attachClass == null || difficultyTrialProgressInfo == null || difficultyTrialProgressInfo.newDifficultyLevel <= difficultyTrialProgressInfo.oldDifficultyLevel)
		{
			yield break;
		}
		DifficultyInfo difficultyInfo = UIBase.applicationState.difficultyManager.GetDifficultyInfo(difficultyTrialProgressInfo.newDifficultyLevel + 1);
		if (difficultyInfo == null)
		{
			yield break;
		}
		string text = base.textData.term_character_class_arg0.FormatWith(record.character.attachClass.characterName);
		string text2 = base.textData.term_difficulty_arg0_unlocked.FormatWith(difficultyInfo.difficultyName);
		BannerHintDisplay bannerHintDisplay = UIBase.gameState.UIManager.bannerHintDisplay;
		if (!(bannerHintDisplay == null))
		{
			yield return bannerHintDisplay.ShowBannerHint(new BannerHintDisplay.SetupInfo
			{
				backgroundType = BannerHintDisplay.BackgroundType.Default,
				soundType = BannerHintDisplay.SoundType.None,
				textType = BannerHintDisplay.TextType.Default,
				hintText = text + "\n" + text2,
				stayTime = 1.5f
			}, 1f);
			yield return new WaitUntil(() => !bannerHintDisplay.gameObject.activeInHierarchy);
		}
	}
}
