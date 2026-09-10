using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionPlayPanel : UIBase, IStackPanel, IShowAsync, IHideAsync
{
	private const int PaidGiftThreshold = 60;

	private const int DailyTaskRewardCount = 1;

	[SerializeField]
	private TextMeshProUGUI activityCountdownText;

	[SerializeField]
	private TextMeshProUGUI helpText;

	[SerializeField]
	private GameObject rulePanel;

	[SerializeField]
	private MatterDef dailyTicketMatter;

	[SerializeField]
	private MatterDef carryTicketMatter;

	[SerializeField]
	private TextMeshProUGUI dailyTicketCountText;

	[SerializeField]
	private TextMeshProUGUI carryTicketCountText;

	[SerializeField]
	private TextMeshProUGUI paidGiftDescriptionText;

	[SerializeField]
	private TextMeshProUGUI paidGiftProgressText;

	[SerializeField]
	private Slider paidGiftProgressSlider;

	[SerializeField]
	private TextMeshProUGUI dailyTaskDescriptionText;

	[SerializeField]
	private GlobalItemDisplay dailyTaskRewardDisplay;

	[SerializeField]
	private GameButton dailyTaskClaimButton;

	[SerializeField]
	private TextMeshProUGUI dailyTaskClaimButtonText;

	[SerializeField]
	private IntermissionPlayRewardCell[] rewardCells;

	[SerializeField]
	private IntermissionPlayCardCell[] cardCells;

	[SerializeField]
	private IntermissionPlayReclaimArea reclaimPanel;

	[SerializeField]
	private GameButton reclaimOpenButton;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private GameButton closeButton;

	private int? revealingCardIndex;

	private int? revealingSlotId;

	private int? winnerSlotId;

	private bool presentingDrawResult;

	private new IntermissionPlayManager manager => UIBase.applicationState.intermissionPlayManager;

	protected override void Init()
	{
		helpButton.onClick.AddListener(delegate
		{
			rulePanel.SetActive(value: true);
			helpText.gameObject.SetActive(value: true);
			ForceUpdate();
		});
		rulePanel.GetComponentInChildren<GameButton>(includeInactive: true).onClick.AddListener(delegate
		{
			rulePanel.SetActive(value: false);
		});
		closeButton.onClick.AddListener(((UIBase)this).HideUI);
		IntermissionPlayRewardCell[] array = rewardCells;
		foreach (IntermissionPlayRewardCell child in array)
		{
			InitUI(child);
		}
		IntermissionPlayCardCell[] array2 = cardCells;
		foreach (IntermissionPlayCardCell child2 in array2)
		{
			InitUI(child2);
		}
		InitUI(reclaimPanel);
		InitUI(dailyTaskRewardDisplay);
		reclaimOpenButton.onClick.AddListener(OpenReclaimPanel);
		dailyTaskClaimButton.onClick.AddListener(delegate
		{
			manager.TryClaimDailyTask();
		});
		manager.StateChanged += OnStateChanged;
		rulePanel.SetActive(value: false);
	}

	protected override void Dispose()
	{
		manager.StateChanged -= OnStateChanged;
	}

	protected override void UpdateDisplay()
	{
		helpText.text = manager.GetLocalizedDescription() + Environment.NewLine + manager.GetLocalizedHelpText();
		dailyTicketCountText.text = "×{0}".FormatWith(manager.Model.DailyTicketCount);
		carryTicketCountText.text = "×{0}".FormatWith(manager.Model.CarryTicketCount);
		paidGiftDescriptionText.text = base.textData.term_real_money_ticket.FormatWith(base.textData.term_creditCurrency, carryTicketMatter.matterName);
		paidGiftProgressText.text = base.textData.term_arg0_per_arg1.FormatWith(manager.Model.PaidGiftProgress, 60);
		paidGiftProgressSlider.value = Mathf.Clamp01((float)manager.Model.PaidGiftProgress / 60f);
		dailyTaskDescriptionText.text = base.textData.term_intermission_play_dancer_task;
		dailyTaskRewardDisplay.Setup(new MatterInfo(dailyTicketMatter, 1));
		dailyTaskClaimButtonText.text = (manager.Model.DailyTicketGranted ? base.textData.term_finished : base.textData.term_newbie_festival_claim);
		dailyTaskClaimButton.interactable = manager.CanClaimDailyTask() && !UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy;
		RefreshRewardCells();
		RefreshCardCells();
		reclaimOpenButton.interactable = true;
		activityCountdownText.text = manager.GetActivityCountdownText();
	}

	private void Update()
	{
		activityCountdownText.text = manager.GetActivityCountdownText();
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		winnerSlotId = null;
		reclaimPanel.Close();
		helpText.gameObject.SetActive(value: false);
		manager.TryFetchInfo(forceRefresh: true);
		yield return new WaitUntil(() => !manager.IsFetchingInfo);
		SetActiveAndDirtyWithFadeAnim(active: true);
		ForceUpdate();
		TryShowPendingPaidTicketRewardPackage();
		ThinkingAnalyticsExtends.TrackIntermissionPlayOpen(UIBase.applicationState, manager.Model.CurrentDayIndex);
	}

	void IHideAsync.OnHideStart()
	{
		helpText.gameObject.SetActive(value: false);
		reclaimPanel.Close();
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void RefreshRewardCells()
	{
		IntermissionPlayRewardCell[] array = rewardCells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		IReadOnlyList<IntermissionPlayRewardSlotDto> rewardSlots = manager.Model.RewardSlots;
		if (rewardSlots == null)
		{
			return;
		}
		foreach (IntermissionPlayRewardSlotDto item in rewardSlots.OrderBy((IntermissionPlayRewardSlotDto slot) => slot.DisplayOrder))
		{
			int num = item.DisplayOrder - 1;
			if (num < 0 || num >= 6)
			{
				throw new InvalidOperationException($"幕间游艺奖励展示顺序非法: {item.DisplayOrder}");
			}
			rewardCells[num].gameObject.SetActive(value: true);
			rewardCells[num].Setup(item, item.SlotId == winnerSlotId, item.SlotId == revealingSlotId);
		}
	}

	private void RefreshCardCells()
	{
		IReadOnlyList<IntermissionPlayOpenedCardDto> openedCards = manager.Model.OpenedCards;
		bool flag = manager.IsOpen() && !manager.IsFetchingInfo && !manager.IsDrawing && !manager.IsReclaiming && !manager.IsClaimingDailyTask && !presentingDrawResult && !UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy && manager.Model.DailyTicketCount + manager.Model.CarryTicketCount > 0 && (manager.Model.RewardSlots?.Any((IntermissionPlayRewardSlotDto slot) => !slot.IsDrawn) ?? false);
		for (int num = 0; num < cardCells.Length; num++)
		{
			int cardIndex = num + 1;
			IntermissionPlayOpenedCardDto intermissionPlayOpenedCardDto = openedCards?.FirstOrDefault((IntermissionPlayOpenedCardDto card) => card.CardIndex == cardIndex);
			if (revealingCardIndex == cardIndex)
			{
				intermissionPlayOpenedCardDto = null;
			}
			cardCells[num].Setup(new IntermissionPlayCardState(cardIndex, intermissionPlayOpenedCardDto, flag && intermissionPlayOpenedCardDto == null));
		}
	}

	private void OpenReclaimPanel()
	{
		reclaimPanel.gameObject.SetActive(value: true);
		reclaimPanel.Setup(new IntermissionPlayReclaimAreaState(manager.Model.OtherworldTicketICount, manager.IsOpen() && !manager.IsFetchingInfo && !manager.IsDrawing && !manager.IsReclaiming && !manager.IsClaimingDailyTask && !presentingDrawResult && !UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy));
	}

	private void OnStateChanged()
	{
		if (base.gameObject.activeInHierarchy && !manager.IsEntranceVisible())
		{
			this.HideUI();
			return;
		}
		SetDirty();
		if (manager.TryConsumePendingDailyTaskRewardPackage(out var taskRewardPackage))
		{
			UIBase.fortressState.StartCoroutine(() => UIBase.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(taskRewardPackage));
		}
		if (manager.TryConsumePendingReclaimResult(out var reclaimResult))
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(base.textData.term_intermission_play_reclaim_success_arg0_arg1_arg2_arg3.FormatWith(reclaimPanel.TicketName, reclaimResult.ReclaimedCount, reclaimPanel.GiftName, reclaimResult.GiftCount));
			UIBase.fortressState.StartCoroutine(() => UIBase.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(new GlobalRewardPackage(reclaimResult.Reward)));
		}
		if (!presentingDrawResult && base.gameObject.activeInHierarchy && manager.TryConsumePendingDrawResult(out var result))
		{
			presentingDrawResult = true;
			revealingCardIndex = result.CardIndex;
			revealingSlotId = result.SlotId;
			winnerSlotId = result.SlotId;
			UIBase.fortressState.StartCoroutine(() => PlayDrawResultCor(result));
		}
	}

	private void TryShowPendingPaidTicketRewardPackage()
	{
		if (base.gameObject.activeInHierarchy && manager.TryConsumePendingPaidTicketCount(out var count))
		{
			GlobalRewardInfo reward = new GlobalRewardInfo
			{
				matter = new MatterInfo(carryTicketMatter, count),
				rewardSourceType = GlobalRewardSourceType.Other,
				rewardSourceId = "#ACT#000003#"
			};
			UIBase.fortressState.StartCoroutine(() => UIBase.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(new GlobalRewardPackage(reward)));
		}
	}

	private IEnumerator PlayDrawResultCor(IntermissionPlayDrawPresentationResult result)
	{
		SetDirty();
		IntermissionPlayOpenedCardDto openedCard = manager.Model.OpenedCards.First((IntermissionPlayOpenedCardDto card) => card.CardIndex == result.CardIndex);
		yield return cardCells[result.CardIndex - 1].PlayFlipCor(openedCard);
		revealingCardIndex = null;
		revealingSlotId = null;
		SetDirty();
		yield return UIBase.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(new GlobalRewardPackage(result.Reward));
		presentingDrawResult = false;
		SetDirty();
	}
}
