using System.Collections;
using Common.UI;
using TMPro;
using UnityEngine;

public class NewbieFestivalPanel : UIBase, IStackPanel, IShowAsync, IHideAsync, IFrameUpdate
{
	[SerializeField]
	private TextMeshProUGUI activityCountdownText;

	[SerializeField]
	private NewbieFestivalWheelPanel wheelPanel;

	[SerializeField]
	private NewbieFestivalTaskPanel taskPanel;

	[SerializeField]
	private GameButton closeButton;

	[SerializeField]
	private GameButton infoButton;

	[SerializeField]
	private GameObject infoBox;

	[SerializeField]
	private GameButton infoBoxCloseButton;

	private NewbieFestivalManager newbieFestivalManager => UIBase.applicationState.newbieFestivalManager;

	protected override void Init()
	{
		InitUI(wheelPanel);
		InitUI(taskPanel);
		closeButton.onClick.AddListener(((UIBase)this).HideUI);
		infoButton.onClick.AddListener(ShowInfoBox);
		infoBoxCloseButton.onClick.AddListener(HideInfoBox);
		newbieFestivalManager.StateChanged += _OnNewbieFestivalStateChanged;
	}

	protected override void Dispose()
	{
		newbieFestivalManager.StateChanged -= _OnNewbieFestivalStateChanged;
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshCountdownTexts();
		}
	}

	void IShowAsync.OnShowStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
		if (!newbieFestivalManager.TryRefreshDailyInfoIfNeeded())
		{
			taskPanel.Setup();
		}
	}

	void IHideAsync.OnHideStart()
	{
		wheelPanel.StopSpin();
		HideInfoBox();
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void RefreshCountdownTexts()
	{
		activityCountdownText.text = newbieFestivalManager.ActivityCountdownText();
	}

	private void _OnNewbieFestivalStateChanged()
	{
		SetDirty();
		if (!wheelPanel.IsSpinning && newbieFestivalManager.TryGetPendingDrawnSlotIndex(out var slotIndex))
		{
			UIBase.fortressState.StartCoroutine(() => PlayDrawResultCor(slotIndex));
		}
		TryShowPendingPaidTicketRewardPackage();
	}

	private void ShowInfoBox()
	{
		infoBox.SetActive(value: true);
	}

	private void HideInfoBox()
	{
		infoBox.SetActive(value: false);
	}

	private IEnumerator PlayDrawResultCor(int slotIndex)
	{
		yield return wheelPanel.PlayDrawResultCor(slotIndex, SetDirty);
		TryShowPendingDrawRewardPackage();
	}

	private void TryShowPendingDrawRewardPackage()
	{
		if (newbieFestivalManager != null && newbieFestivalManager.TryConsumePendingDrawRewardPackage(out var package))
		{
			SetDirty();
			UIBase.fortressState.StartCoroutine(() => UIBase.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(package));
		}
	}

	private void TryShowPendingPaidTicketRewardPackage()
	{
		if (newbieFestivalManager != null && newbieFestivalManager.TryConsumePendingPaidTicketRewardPackage(out var package))
		{
			SetDirty();
			UIBase.fortressState.StartCoroutine(() => UIBase.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(package));
		}
	}
}
