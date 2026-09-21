using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using CoreKit.Agent;
using UnityEngine;

public class PleaseReviewPopup : UIBase
{
	private const int PopupTypeCompliance = 0;

	private const int PopupTypeViolation = 1;

	private const int FirstPopupIndex = 1;

	private const int SecondPopupIndex = 2;

	[SerializeField]
	private GameObject firstPop;

	[SerializeField]
	private GameObject secondPop;

	[SerializeField]
	private GameObject violationFirstPop;

	[SerializeField]
	private GameObject violationSecondPop;

	[SerializeField]
	private GameButton firstToReviewButton;

	[SerializeField]
	private GameButton firstBackButton;

	[SerializeField]
	private GameButton secondToReviewButton;

	[SerializeField]
	private GameButton secondBackButton;

	[SerializeField]
	private GameButton violationFirstToReviewButton;

	[SerializeField]
	private GameButton violationFirstBackButton;

	[SerializeField]
	private GameButton violationSecondToReviewButton;

	[SerializeField]
	private GameButton violationSecondBackButton;

	[SerializeField]
	private CellUIPrefabPoolConfig<GlobalItemDisplay, IGlobalItemDisplaySetup> violationFirstRewardPool;

	[SerializeField]
	private CellUIPrefabPoolConfig<GlobalItemDisplay, IGlobalItemDisplaySetup> violationSecondRewardPool;

	private int currentPopupType;

	private int currentPopupIndex;

	protected override void Init()
	{
		if (violationFirstRewardPool != null)
		{
			Init(violationFirstRewardPool);
		}
		if (violationSecondRewardPool != null)
		{
			Init(violationSecondRewardPool);
		}
		foreach (GameButton item in new HashSet<GameButton> { firstToReviewButton, secondToReviewButton, violationFirstToReviewButton, violationSecondToReviewButton }.Where((GameButton button) => button != null))
		{
			item.onClick.AddListener(_OnToReviewButtonClicked);
		}
		foreach (GameButton item2 in new HashSet<GameButton> { firstBackButton, secondBackButton, violationFirstBackButton, violationSecondBackButton }.Where((GameButton button) => button != null))
		{
			item2.onClick.AddListener(_OnBackButtonClicked);
		}
	}

	public void ShowFirstPop(int popupType)
	{
		currentPopupIndex = 1;
		ShowPop(popupType, firstPop, violationFirstPop);
		SetActiveAndDirtyWithFadeAnim(active: true);
	}

	public void ShowSecondPop(int popupType)
	{
		currentPopupIndex = 2;
		ShowPop(popupType, secondPop, violationSecondPop);
		SetActiveAndDirtyWithFadeAnim(active: true);
	}

	private void ShowPop(int popupType, GameObject compliancePop, GameObject violationPop)
	{
		currentPopupType = popupType;
		firstPop.SetActive(value: false);
		secondPop.SetActive(value: false);
		violationFirstPop.SetActive(value: false);
		violationSecondPop.SetActive(value: false);
		switch (popupType)
		{
		case 0:
			compliancePop.SetActive(value: true);
			break;
		case 1:
			violationPop.SetActive(value: true);
			if (currentPopupIndex == 1)
			{
				violationFirstRewardPool.Sync(GetViolationFirstRewards());
			}
			else
			{
				violationSecondRewardPool.Sync(GetViolationSecondRewards());
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("popupType", popupType, "Invalid please review popup type");
		}
	}

	private void _OnToReviewButtonClicked()
	{
		AgentHub.Instance.Special.callReview();
		if (currentPopupType == 1)
		{
			ClaimViolationReward();
		}
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnBackButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void ClaimViolationReward()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		ClaimPleaseReviewPopupRewardRequest request = new ClaimPleaseReviewPopupRewardRequest
		{
			PopupIndex = currentPopupIndex
		};
		GeneralGameAsyncOperationResultOnly<ClaimPleaseReviewPopupRewardResult> claimOp = new GeneralGameAsyncOperationResultOnly<ClaimPleaseReviewPopupRewardResult>(() => UIBase.applicationState.ChillyRoomSDK.PleaseReviewPopup.ClaimPleaseReviewPopupRewardAsync(request));
		claimOp.OnComplete(delegate
		{
			GlobalRewardPackage globalRewardPackage = claimOp.OpResult.GiveRewards(UIBase.applicationState);
			if (globalRewardPackage != null)
			{
				UIBase.applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(globalRewardPackage);
			}
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_submitting_review_feedback);
	}

	private List<IGlobalItemDisplaySetup> GetViolationFirstRewards()
	{
		return new List<IGlobalItemDisplaySetup>
		{
			new MatterReward(UIBase.applicationState.assetCatalog.FullIdToCTAsset<MatterDef>("#MT#000004#"), 600),
			new MatterReward(UIBase.applicationState.assetCatalog.FullIdToCTAsset<MatterDef>("#MT#000013#"), 15)
		};
	}

	private List<IGlobalItemDisplaySetup> GetViolationSecondRewards()
	{
		return new List<IGlobalItemDisplaySetup>
		{
			new MatterReward(UIBase.applicationState.assetCatalog.FullIdToCTAsset<MatterDef>("#MT#000123#"), 5)
		};
	}
}
