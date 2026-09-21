using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class YuanQiActivityMissionCell : CellUI<YuanQiActivityMissionItem>
{
	private const int MaxRewardDisplayCount = 2;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private List<YuanQiMissionRewardStrip> rewardStrips = new List<YuanQiMissionRewardStrip>();

	[SerializeField]
	private GameButton goButton;

	[SerializeField]
	private GameButton claimButton;

	[SerializeField]
	private GameObject hasClaimObj;

	[SerializeField]
	private GameObject lockGameObject;

	[SerializeField]
	private TextMeshProUGUI unlockCondition;

	[NonSerialized]
	private Action<YuanQiActivityMissionItem> onGo;

	[NonSerialized]
	private Action<YuanQiActivityMissionItem> onClaim;

	private YuanQiActivityMissionItem missionItem => base.setupData;

	protected override void Init()
	{
		foreach (YuanQiMissionRewardStrip rewardStrip in rewardStrips)
		{
			if (rewardStrip != null)
			{
				InitUI(rewardStrip);
			}
		}
		if (goButton != null)
		{
			goButton.onClick.AddListener(OnGoClicked);
		}
		if (claimButton != null)
		{
			claimButton.onClick.AddListener(OnClaimClicked);
		}
	}

	protected override void Dispose()
	{
		if (goButton != null)
		{
			goButton.onClick.RemoveListener(OnGoClicked);
		}
		if (claimButton != null)
		{
			claimButton.onClick.RemoveListener(OnClaimClicked);
		}
		onGo = null;
		onClaim = null;
	}

	protected override void UpdateDisplay()
	{
		titleText.text = GetTextByKey(missionItem.Data.titleKey);
		descriptionText.text = GetTextByKey(missionItem.Data.descriptionKey);
		progressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(missionItem.Progress, missionItem.Data.targetProgress);
		UpdateRewards();
		UpdateButtons();
		UpdateLockState();
	}

	public void SetActions(Action<YuanQiActivityMissionItem> goAction, Action<YuanQiActivityMissionItem> claimAction)
	{
		onGo = goAction;
		onClaim = claimAction;
	}

	private void UpdateRewards()
	{
		IReadOnlyList<MatterReward> rewards = missionItem.Data.Rewards;
		bool isClaimed = missionItem.Status == YuanQiActivityMissionStatus.Claimed;
		for (int i = 0; i < rewardStrips.Count; i++)
		{
			YuanQiMissionRewardStrip yuanQiMissionRewardStrip = rewardStrips[i];
			if (!(yuanQiMissionRewardStrip == null))
			{
				bool flag = i < 2 && rewards != null && i < rewards.Count && rewards[i] != null;
				yuanQiMissionRewardStrip.gameObject.SetActive(flag);
				if (flag)
				{
					yuanQiMissionRewardStrip.Setup(rewards[i], isClaimed);
				}
			}
		}
	}

	private void UpdateButtons()
	{
		goButton.gameObject.SetActive(missionItem.CanJump);
		goButton.interactable = missionItem.CanJump;
		claimButton.gameObject.SetActive(missionItem.CanClaim);
		claimButton.interactable = missionItem.CanClaim;
		hasClaimObj.SetActive(missionItem.HasClaim);
	}

	private void UpdateLockState()
	{
		bool flag = missionItem.Status == YuanQiActivityMissionStatus.Locked;
		if (lockGameObject != null)
		{
			lockGameObject.SetActive(flag);
		}
		if (!(unlockCondition == null))
		{
			unlockCondition.gameObject.SetActive(flag);
			if (flag)
			{
				YuanQiActivityMissionItem previousMissionItem = CellUI<YuanQiActivityMissionItem>.applicationState.yuanQiActivityManager.Model.GetPreviousMissionItem(missionItem.Data.order);
				string text = ((previousMissionItem == null) ? string.Empty : GetTextByKey(previousMissionItem.Data.titleKey));
				unlockCondition.text = (string.IsNullOrEmpty(text) ? string.Empty : ScriptableObjectSingleton<TextDataPreset>.instance.term_activity_unlock_condition.FormatWith(text));
			}
		}
	}

	private void OnGoClicked()
	{
		if (missionItem != null)
		{
			onGo?.Invoke(missionItem);
		}
	}

	private void OnClaimClicked()
	{
		if (missionItem != null)
		{
			onClaim?.Invoke(missionItem);
		}
	}

	private static string GetTextByKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		string serverTerm = TextDataPreset.GetServerTerm(key);
		if (!string.IsNullOrEmpty(serverTerm))
		{
			return serverTerm;
		}
		return (typeof(TextDataPreset).GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(ScriptableObjectSingleton<TextDataPreset>.instance) as string) ?? key;
	}
}
