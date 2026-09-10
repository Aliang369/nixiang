using System;
using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Fortress;
using UnityEngine;

public sealed class FortressChallengeReturnTransition
{
	private enum TransitionState
	{
		Pending,
		Cancelled,
		Completed
	}

	private const float TimeoutSeconds = 4f;

	private readonly ApplicationStateInfo applicationState;

	private readonly BootStrapLoadData loadData;

	private readonly FortressReturnTransitionCover cover;

	private TransitionState state;

	public bool IsPending => state == TransitionState.Pending;

	public FortressChallengeReturnTransition(ApplicationStateInfo applicationState, BootStrapLoadData loadData, FortressReturnTransitionCover cover)
	{
		this.applicationState = applicationState;
		this.loadData = loadData;
		this.cover = cover;
	}

	public void Arm()
	{
		cover.Show(4f, delegate
		{
			Cancel("timeout");
		});
	}

	public bool Cancel(string reason)
	{
		if (!IsPending)
		{
			return false;
		}
		state = TransitionState.Cancelled;
		cover.HideImmediate();
		TraceLog.Debug("[FortressChallengeReturnTransition] cancelled: " + reason);
		return true;
	}

	public bool CancelIfVisibleInterruption(string reason)
	{
		if (HasVisibleInterruption())
		{
			return Cancel(reason);
		}
		return false;
	}

	public bool TryRestoreTarget(Action onRevealed)
	{
		if (!IsPending)
		{
			return false;
		}
		FortressUIManager fortressUIManager = (applicationState?.fortressState)?.uiManager;
		FortressMainUIPanel fortressMainUIPanel = fortressUIManager?.mainUIPanel;
		MainUISwitchGroup mainUISwitchGroup = fortressMainUIPanel?.mainUISwitchGroup;
		StarMapPanel starMapPanel = fortressUIManager?.starMapPanel;
		if (fortressUIManager == null || fortressMainUIPanel == null || mainUISwitchGroup == null || starMapPanel == null)
		{
			return CancelAndFail("fortress UI unavailable");
		}
		if (fortressUIManager.transforming || fortressUIManager.currentPanel != fortressMainUIPanel || mainUISwitchGroup.IsSwitching)
		{
			return CancelAndFail("fortress UI is busy");
		}
		if (mainUISwitchGroup.currentLayerSetups == null || mainUISwitchGroup.currentLayerSetups.layer != FortressMainUIPanel.MainPanelLayer.MainRoom)
		{
			return CancelAndFail("fortress is not in main room");
		}
		if (HasVisibleInterruption())
		{
			return CancelAndFail("visible entry presentation");
		}
		FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup = mainUISwitchGroup.mainPanelLayerSetups?.FirstOrDefault((FortressMainUIPanel.MainPanelLayerSetup setup) => setup != null && setup.layer == FortressMainUIPanel.MainPanelLayer.StargazingRoom);
		if (mainPanelLayerSetup == null || mainPanelLayerSetup.isLocked)
		{
			return CancelAndFail("stargazing room unavailable");
		}
		if (WouldTriggerStargazingLayerPresentation())
		{
			return CancelAndFail("pending stargazing layer presentation");
		}
		Constellation constellation = null;
		switch (loadData.fortressReturnTarget)
		{
		case BootStrapLoadData.FortressReturnTarget.DailyChallenge:
			if (!starMapPanel.CanOpenDailyChallenge())
			{
				return CancelAndFail("daily challenge unavailable");
			}
			break;
		case BootStrapLoadData.FortressReturnTarget.ChallengeConstellation:
			if (!starMapPanel.TryResolveOpenConstellation(loadData.fortressReturnContractDef, out constellation))
			{
				return CancelAndFail("challenge constellation unavailable");
			}
			break;
		default:
			return CancelAndFail("return target missing");
		}
		if (!mainUISwitchGroup.TrySwitchToLayer(FortressMainUIPanel.MainPanelLayer.StargazingRoom, animated: false))
		{
			return CancelAndFail("stargazing room switch failed");
		}
		if (HasVisibleInterruption())
		{
			return CancelAndFail("stargazing room presentation triggered");
		}
		if (!((loadData.fortressReturnTarget == BootStrapLoadData.FortressReturnTarget.DailyChallenge) ? starMapPanel.TryOpenDailyChallenge() : OpenConstellation(starMapPanel, constellation)))
		{
			return CancelAndFail("target page failed to open");
		}
		float hidePanelTime = GlobalConfigManager.instance.loadingPanelConfig.hidePanelTime;
		cover.HideWithFade(hidePanelTime, onRevealed);
		state = TransitionState.Completed;
		TraceLog.Debug($"[FortressChallengeReturnTransition] restored: {loadData.fortressReturnTarget}");
		return true;
	}

	private bool CancelAndFail(string reason)
	{
		Cancel(reason);
		return false;
	}

	private bool HasVisibleInterruption()
	{
		if (!(applicationState?.storyPointManager?.RunningStoryPoint != null) && applicationState?.currentCutSceneState == null)
		{
			BootstrapExtra instance = BootstrapExtra.instance;
			if ((object)instance == null || instance.gameAsyncOperationManager?.IsBlockingOperationRunning != true)
			{
				FortressUIManager fortressUIManager = applicationState?.fortressState?.uiManager;
				if (fortressUIManager == null)
				{
					return false;
				}
				ClavisHint clavisHint = fortressUIManager.clavisHint;
				if (((object)clavisHint == null || !clavisHint.WithClavisToHint) && !IsActive(fortressUIManager.globalRewardPackageDisplay) && !IsActive(fortressUIManager.festivalCheckInPanel) && !IsActive(fortressUIManager.carnivalCheckInPanel) && !IsActive(fortressUIManager.monthCheckPanel) && !IsActive(fortressUIManager.limitedTimeBundlePanel) && !IsActive(fortressUIManager.purchasePromotionPanel) && !IsActive(fortressUIManager.bindAccountLoginHint) && !IsActive(fortressUIManager.commonSecondConfirmPanel))
				{
					return IsActive(fortressUIManager.pleaseReviewPopup);
				}
				return true;
			}
		}
		return true;
	}

	private bool WouldTriggerStargazingLayerPresentation()
	{
		List<StoryPoint> list = applicationState?.assetCatalog?.storyPointCatalog?.ValidAssets;
		if (list != null)
		{
			foreach (StoryPoint item in list)
			{
				if (!(item == null) && !item.IsFinished(applicationState) && item.triggerObserver is EnterFortressMainPanelLayer { targetLayer: FortressMainUIPanel.MainPanelLayer.StargazingRoom } && (item.parentPoints == null || item.parentPoints.All((StoryPoint parent) => parent != null && parent.IsFinished(applicationState))))
				{
					StoryPointManager.StoryPointState storyState = applicationState.storyPointManager.GetStoryState(item);
					int num = Mathf.Max(1, item.observeCountRequirement);
					if (storyState.observeCount + 1 >= num)
					{
						return true;
					}
				}
			}
		}
		return (applicationState?.assetCatalog?.clavisDefDefCatalog?.ValidAssets)?.Any((ClavisDef clavis) => clavis != null && !applicationState.clavisManager.IsUnlocked(clavis) && clavis.unlockObserver is EnterFortressMainPanelLayer enterFortressMainPanelLayer2 && enterFortressMainPanelLayer2.targetLayer == FortressMainUIPanel.MainPanelLayer.StargazingRoom) ?? false;
	}

	private static bool OpenConstellation(StarMapPanel starMapPanel, Constellation constellation)
	{
		if (constellation == null)
		{
			return false;
		}
		starMapPanel.OpenConstellation(constellation);
		return true;
	}

	private static bool IsActive(Component component)
	{
		if (component != null)
		{
			return component.gameObject.activeInHierarchy;
		}
		return false;
	}
}
