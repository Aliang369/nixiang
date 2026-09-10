using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewbieFestivalWheelPanel : UIBase
{
	[SerializeField]
	private TextMeshProUGUI dayRefreshCountdownText;

	[SerializeField]
	private TextMeshProUGUI freeTicketText;

	[SerializeField]
	private TextMeshProUGUI paidTicketText;

	[SerializeField]
	private TextMeshProUGUI drawButtonText;

	[SerializeField]
	private TextMeshProUGUI drawCostText;

	[SerializeField]
	private Image drawCostIcon;

	[SerializeField]
	private NewbieFestivalRewardCell[] rewardCells;

	[SerializeField]
	private RectTransform wheelTransform;

	[SerializeField]
	private int spinRoundCount = 5;

	[SerializeField]
	private float spinDuration = 2.8f;

	[SerializeField]
	private float wheelSlotAngleOffset;

	[SerializeField]
	private GameButton drawButton;

	private Action drawClicked;

	private NewbieFestivalManager newbieFestivalManager => UIBase.applicationState.newbieFestivalManager;

	public bool IsSpinning { get; private set; }

	protected override void Init()
	{
		drawButton.onClick.AddListener(_OnDrawButtonClicked);
		NewbieFestivalRewardCell[] array = rewardCells;
		foreach (NewbieFestivalRewardCell child in array)
		{
			InitUI(child);
		}
	}

	protected override void Dispose()
	{
		drawButton.onClick.RemoveListener(_OnDrawButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		drawCostText.text = "×{0}".FormatWith(1);
		drawCostIcon.sprite = newbieFestivalManager.nextDrawTicketMatter.icon;
		freeTicketText.text = "×{0}".FormatWith(newbieFestivalManager.freeTicketCount);
		paidTicketText.text = "×{0}".FormatWith(newbieFestivalManager.paidTicketCount);
		dayRefreshCountdownText.text = newbieFestivalManager.CurrentDayRefreshTimeCountdownText();
		RefreshRewardCells();
		RefreshDrawButton(UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy);
	}

	public void RefreshRewardCells()
	{
		NewbieFestivalManager obj = newbieFestivalManager;
		NewbieFestivalRewardCell[] array;
		if (obj == null || !obj.hasActivityData)
		{
			array = rewardCells;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
			return;
		}
		array = rewardCells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		NewbieFestivalDailyRewardGroup currentDailyRewardGroup = newbieFestivalManager.GetCurrentDailyRewardGroup();
		List<int> list = newbieFestivalManager.drawnSlotIndexes;
		if (newbieFestivalManager.TryGetPendingDrawnSlotIndex(out var pendingDrawnSlotIndex))
		{
			list = list?.Where((int slotIndex) => slotIndex != pendingDrawnSlotIndex).ToList();
		}
		IEnumerable<NewbieFestivalRewardSlot> enumerable = currentDailyRewardGroup?.rewardSlots.OrderBy((NewbieFestivalRewardSlot rewardSlot) => rewardSlot.slotIndex);
		foreach (NewbieFestivalRewardSlot item in enumerable ?? Enumerable.Empty<NewbieFestivalRewardSlot>())
		{
			int num = item.slotIndex - 1;
			rewardCells[num].gameObject.SetActive(value: true);
			rewardCells[num].Setup(item, (typeof(List<int>), list));
		}
	}

	public void RefreshDrawButton(bool rewardDisplayActive)
	{
		bool hasActivityData = newbieFestivalManager.hasActivityData;
		bool isAvailable = newbieFestivalManager.isAvailable;
		bool interactable = (hasActivityData & isAvailable) && !newbieFestivalManager.drawing && !rewardDisplayActive && !IsSpinning && newbieFestivalManager.GetRemainingRewardSlots() > 0 && newbieFestivalManager.freeTicketCount + newbieFestivalManager.paidTicketCount > 0;
		drawButton.interactable = interactable;
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (!isAvailable)
		{
			drawButtonText.text = instance.term_newbie_festival_finished;
		}
		else if (!hasActivityData)
		{
			drawButtonText.text = (newbieFestivalManager.fetchingInfo ? instance.term_newbie_festival_loading : instance.term_newbie_festival_no_data);
		}
		else if (newbieFestivalManager.drawing || IsSpinning)
		{
			drawButtonText.text = instance.term_newbie_festival_drawing;
		}
		else if (rewardDisplayActive)
		{
			drawButtonText.text = instance.term_newbie_festival_reward_displaying;
		}
		else
		{
			drawButtonText.text = instance.term_newbie_festival_draw_once;
		}
	}

	public void StopSpin()
	{
		wheelTransform.DOKill();
		IsSpinning = false;
	}

	public IEnumerator PlayDrawResultCor(int slotIndex, Action setDirty)
	{
		IsSpinning = true;
		setDirty();
		float z = wheelTransform.localEulerAngles.z;
		float target = 0f - Vector2.SignedAngle(to: rewardCells[slotIndex - 1].rectTransform.anchoredPosition, from: Vector2.right);
		float z2 = z + Mathf.DeltaAngle(z, target) - 360f * (float)spinRoundCount;
		wheelTransform.DOKill();
		wheelTransform.DOLocalRotate(new Vector3(0f, 0f, z2), spinDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuart);
		yield return new WaitForSeconds(spinDuration);
		IsSpinning = false;
		setDirty();
	}

	private void _OnDrawButtonClicked()
	{
		if (UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_close_reward_first);
		}
		else if (!UIBase.applicationState.newbieFestivalManager.TryDraw())
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_cannot_draw);
			SetDirty();
		}
		else
		{
			SetDirty();
		}
	}
}
