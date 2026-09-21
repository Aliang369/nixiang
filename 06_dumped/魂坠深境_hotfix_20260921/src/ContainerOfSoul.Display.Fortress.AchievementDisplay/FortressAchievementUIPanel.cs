using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.AchievementDisplay;

public class FortressAchievementUIPanel : UIBase, IDisplayModulesController, IShowAsync, IHideAsync
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameObject achievementPanel;

	[SerializeField]
	private AchievementCell achievementCellPrefab;

	[SerializeField]
	private GameButton receiveAllAchievementRewardButton;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[Header("第一维标签页")]
	[SerializeField]
	private SwitchPanel switchPanel;

	[SerializeField]
	private SwitchImageSwitchTag switchTagPrefab;

	[Header("第二维标签页")]
	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[NonSerialized]
	private CellUIPrefabPool<AchievementCell, Achievement> achievementCellPool;

	[NonSerialized]
	private CellUIPrefabPool<SwitchImageSwitchTag, SwitchTag.Data> switchTagPool;

	[NonSerialized]
	private bool claimingAchievementRewards;

	private IEnumerable<Achievement> DisplayAchievements => UIBase.applicationState.achievementManager.globalAchievements;

	private IEnumerable<Achievement> AchievementsWaitForGetReward => DisplayAchievements.Where((Achievement ach) => ach.GetRewardAble);

	public bool CanClaimAchievementRewards => !claimingAchievementRewards;

	protected override void Init()
	{
		backButton.onClick.AddListener(((UIBase)this).HideUI);
		UIBase.applicationState.eventManager.achievementAchieveEvent.AddListener(_OnAchievementAchieved);
		GetPrefabPool(ref achievementCellPool, achievementCellPrefab);
		UIBase.applicationState.eventManager.RoutineQuestRefreshedEvent.AddListener(_OnRoutineQuestRefreshed);
		UIBase.applicationState.eventManager.matterChangeEvent.AddListener(_OnMatterChange);
		TextMeshProUGUI textMeshProUGUI = (base.transform.Find("TitleImage/Title") ?? base.transform.Find("PanelArea/TitleImage/Title"))?.GetComponent<TextMeshProUGUI>();
		if (textMeshProUGUI != null)
		{
			textMeshProUGUI.text = base.textData.term_achievement_title;
		}
		receiveAllAchievementRewardButton.onClick.AddListener(ClaimCurrentPageAchievementRewards);
		_InitSwitchPanel();
		_InitCharacterClassFilterPanel();
		SetDirty();
		void _InitCharacterClassFilterPanel()
		{
			InitUI(characterClassFilterPanel);
			List<IconObjectFilterPanel.IconFilterData> list = (from c in UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills)
				where c.target is CharacterClassDef characterClassDef2 && HasCharacterClassAchievement(characterClassDef2)
				select c).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				IconObjectFilterPanel.IconFilterData value = list[num];
				if (value.target is CharacterClassDef characterClassDef)
				{
					CharacterClassDef targetCharacterClassDef = characterClassDef;
					value.shouldShowRedDot = () => HasRewardableCharacterClassAchievement(targetCharacterClassDef);
				}
				list[num] = value;
			}
			characterClassFilterPanel.Sync(list);
			characterClassFilterPanel.canUnselect = false;
			if (list.Count > 0)
			{
				characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(list[0].target);
			}
			characterClassFilterPanel.onChangeSelected = _OnCharacterClassFilterChanged;
		}
		void _InitSwitchPanel()
		{
			InitUI(switchPanel);
			switchPanel.ClearTags();
			GetPrefabPool(ref switchTagPool, switchTagPrefab);
			SwitchImageSwitchTag switchImageSwitchTag = switchTagPool.Spawn(new SwitchTag.Data
			{
				i = 0,
				shouldShowRedDot = (SwitchTag _) => HasRewardableAchievementInTab(0)
			});
			switchImageSwitchTag.SetText(base.textData.term_journey);
			switchPanel.AddTag(switchImageSwitchTag);
			switchImageSwitchTag = switchTagPool.Spawn(new SwitchTag.Data
			{
				i = 1,
				shouldShowRedDot = (SwitchTag _) => HasRewardableAchievementInTab(1)
			});
			switchImageSwitchTag.SetText(base.textData.term_class);
			switchPanel.AddTag(switchImageSwitchTag);
			switchImageSwitchTag = switchTagPool.Spawn(new SwitchTag.Data
			{
				i = 2,
				shouldShowRedDot = (SwitchTag _) => HasRewardableAchievementInTab(2)
			});
			switchImageSwitchTag.SetText(base.textData.term_challenge);
			switchPanel.AddTag(switchImageSwitchTag);
			SwitchPanel obj = switchPanel;
			obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
			switchPanel.Select(0);
		}
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.achievementAchieveEvent.RemoveListener(_OnAchievementAchieved);
		UIBase.applicationState.eventManager.RoutineQuestRefreshedEvent.RemoveListener(_OnRoutineQuestRefreshed);
		UIBase.applicationState.eventManager.matterChangeEvent.RemoveListener(_OnMatterChange);
	}

	protected override void UpdateDisplay()
	{
		List<Achievement> elements = (from a in UIBase.applicationState.achievementManager.globalAchievements.Where(LayerAccept)
			orderby (int)(1000 * ((!a.GetRewardAble) ? ((!a.achieved) ? 1 : 2) : 0) + a.Layer)
			select a).ToList();
		achievementCellPool.UpdateSequenceDisplay(elements, layoutGroup);
		receiveAllAchievementRewardButton.transform.parent.gameObject.SetActive(value: false);
		int i = switchPanel.targetShowTag.setupData.i;
		characterClassFilterPanel.gameObject.SetActive(i == 1);
		bool LayerAccept(Achievement achievement)
		{
			if (switchPanel.targetShowTag.setupData.i == 0)
			{
				if (achievement.Layer != AchievementDef.AchievementLayer.Intro)
				{
					return achievement.Layer == AchievementDef.AchievementLayer.Progress;
				}
				return true;
			}
			if (switchPanel.targetShowTag.setupData.i == 2)
			{
				return achievement.Layer == AchievementDef.AchievementLayer.Special;
			}
			if (achievement.Layer != AchievementDef.AchievementLayer.CharacterClass)
			{
				return false;
			}
			if (characterClassFilterPanel.selected == null)
			{
				return false;
			}
			return achievement.achievementDef.attachCharacterClass == characterClassFilterPanel.selected.setupData.target;
		}
	}

	public int GetNeedReceiveAchievementAndQuestCount()
	{
		return 0 + AchievementsWaitForGetReward.Count();
	}

	private bool HasRewardableAchievementInTab(int tabIndex)
	{
		return DisplayAchievements.Any((Achievement achievement) => achievement.GetRewardAble && LayerAccept(achievement, tabIndex));
	}

	private bool HasRewardableCharacterClassAchievement(CharacterClassDef characterClassDef)
	{
		return DisplayAchievements.Any((Achievement achievement) => achievement.GetRewardAble && achievement.Layer == AchievementDef.AchievementLayer.CharacterClass && achievement.achievementDef.attachCharacterClass == characterClassDef);
	}

	private bool HasCharacterClassAchievement(CharacterClassDef characterClassDef)
	{
		return DisplayAchievements.Any((Achievement achievement) => achievement.Layer == AchievementDef.AchievementLayer.CharacterClass && achievement.achievementDef.attachCharacterClass == characterClassDef);
	}

	private static bool LayerAccept(Achievement achievement, int tabIndex)
	{
		switch (tabIndex)
		{
		case 0:
			if (achievement.Layer != AchievementDef.AchievementLayer.Intro)
			{
				return achievement.Layer == AchievementDef.AchievementLayer.Progress;
			}
			return true;
		case 2:
			return achievement.Layer == AchievementDef.AchievementLayer.Special;
		default:
			return achievement.Layer == AchievementDef.AchievementLayer.CharacterClass;
		}
	}

	private void _OnTagSwitched()
	{
		SetDirty();
	}

	private void _OnCharacterClassFilterChanged()
	{
		SetDirty();
	}

	private void _OnAchievementAchieved(Achievement ad)
	{
		SetDirty();
	}

	private void _OnRoutineQuestRefreshed()
	{
		SetDirty();
	}

	private void _OnMatterChange(MatterInfo info)
	{
		SetDirty();
	}

	public void ClaimAchievementRewards(Achievement selectedAchievement)
	{
		if (selectedAchievement != null)
		{
			Achievement[] achievements = ((selectedAchievement.Layer != AchievementDef.AchievementLayer.CharacterClass) ? AchievementsWaitForGetReward.Where((Achievement achievement) => IsSameBatchPage(achievement, selectedAchievement)).ToArray() : new Achievement[1] { selectedAchievement });
			ClaimAchievementRewards(achievements);
		}
	}

	private void ClaimCurrentPageAchievementRewards()
	{
		int tabIndex = switchPanel.targetShowTag.setupData.i;
		if (tabIndex != 1)
		{
			ClaimAchievementRewards(AchievementsWaitForGetReward.Where((Achievement achievement) => LayerAccept(achievement, tabIndex)).ToArray());
		}
	}

	private static bool IsSameBatchPage(Achievement achievement, Achievement selectedAchievement)
	{
		if (selectedAchievement.Layer == AchievementDef.AchievementLayer.Special)
		{
			return achievement.Layer == AchievementDef.AchievementLayer.Special;
		}
		if (achievement.Layer != AchievementDef.AchievementLayer.Intro)
		{
			return achievement.Layer == AchievementDef.AchievementLayer.Progress;
		}
		return true;
	}

	private void ClaimAchievementRewards(Achievement[] achievements)
	{
		if (claimingAchievementRewards)
		{
			return;
		}
		achievements = achievements?.Where((Achievement achievement) => achievement?.GetRewardAble ?? false).Distinct().ToArray();
		if (achievements.IsNullOrEmpty())
		{
			return;
		}
		claimingAchievementRewards = true;
		SetDirty();
		AchievementDef[] achievementDefs;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			achievementDefs = achievements.Select((Achievement a) => a.achievementDef).ToArray();
			UIBase.applicationState.achievementManager.EnsureAchievementsUploaded(achievementDefs, RequestRewards);
		}
		void AfterRewardReceived(GlobalRewardPackage rewardPack)
		{
			if (rewardPack != null)
			{
				rewardPack.rewards = GlobalRewardInfo.CompressRewardsForDisplay(rewardPack.rewards);
				UIBase.applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(rewardPack);
			}
			Achievement[] array = achievements;
			foreach (Achievement achievement in array)
			{
				ThinkingAnalyticsExtends.TrackAchievementFinish(UIBase.applicationState, achievement);
			}
			FinishClaim();
		}
		void FinishClaim()
		{
			claimingAchievementRewards = false;
			SetDirty();
			UIBase.fortressState.uiManager.mainUIPanel.SetDirty();
		}
		void RequestRewards()
		{
			GetAchievementBonusRequest request = new GetAchievementBonusRequest(achievementDefs);
			GeneralGameAsyncOperation<GetAchievementBonusRequest, GetAchievementBonusResult> op = new GeneralGameAsyncOperation<GetAchievementBonusRequest, GetAchievementBonusResult>(request, UIBase.applicationState.ChillyRoomSDK.Progress.GetAchievementsBonusAsync);
			bool operationSucceeded = false;
			op.OnComplete(delegate
			{
				operationSucceeded = true;
				GlobalRewardPackage rewardPack = op.OpResult.GiveAchievementRewards(UIBase.applicationState);
				AfterRewardReceived(rewardPack);
			}).OnOver(delegate
			{
				if (!operationSucceeded)
				{
					FinishClaim();
				}
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_quest_reward);
		}
	}

	void IShowAsync.OnShowStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
	}

	void IHideAsync.OnHideStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}
}
