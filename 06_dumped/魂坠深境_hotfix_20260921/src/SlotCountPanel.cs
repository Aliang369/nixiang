using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotCountPanel : UIBase
{
	[SerializeField]
	private Image glow;

	[SerializeField]
	private RectTransform pivot;

	[SerializeField]
	private TextMeshProUGUI normalPanelFirstDrawCountText;

	[SerializeField]
	private TextMeshProUGUI normalPanelSlotCountText;

	[SerializeField]
	private TextMeshProUGUI normalPanelDrawCountText;

	[SerializeField]
	private GameButton normalPanelButton;

	[SerializeField]
	private GameObject normalPanel;

	[SerializeField]
	private TextMeshProUGUI battlePanelSlotCountText;

	[SerializeField]
	private GameButton battlePanelButton;

	[SerializeField]
	private GameObject battlePanel;

	[SerializeField]
	private GameObject infoPanel;

	[SerializeField]
	private TextMeshProUGUI infoPanelFirstDrawCountText;

	[SerializeField]
	private TextMeshProUGUI infoPanelSlotCountText;

	[SerializeField]
	private TextMeshProUGUI infoPanelDrawCountText;

	[NonSerialized]
	private int fixedSlotCount;

	[NonSerialized]
	private bool inBattle;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private bool showInfo;

	[NonSerialized]
	private int slotCount;

	protected override void Init()
	{
		slotCount = -1;
		fixedSlotCount = -1;
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _OnBattleStart);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		UIBase.gameState.actionManager.actionQueueCleanEvent += _OnActionQueueCleanEvent;
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		normalPanelButton.onClick.AddListener(_OnButtonClicked);
		battlePanelButton.onClick.AddListener(_OnButtonClicked);
		showInfo = false;
		inBattle = false;
		infoPanel.SetActive(value: false);
		normalPanel.SetActive(value: true);
		battlePanel.SetActive(value: false);
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _OnBattleStart);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		UIBase.gameState.actionManager.actionQueueCleanEvent -= _OnActionQueueCleanEvent;
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	public void ShowInfo()
	{
		if (!showInfo)
		{
			showInfo = true;
			infoPanel.SetActive(value: true);
			SetDirty();
		}
	}

	public void HideInfo()
	{
		if (showInfo)
		{
			showInfo = false;
			infoPanel.SetActive(value: false);
			SetDirty();
		}
	}

	protected override void UpdateDisplay()
	{
		if (inBattle)
		{
			UpdateDisplay_Battle();
		}
		else
		{
			UpdateDisplay_Normal();
		}
		if (showInfo)
		{
			UpdateDisplay_Info();
		}
	}

	private void UpdateDisplay_Normal()
	{
		normalPanelFirstDrawCountText.text = $"{UIBase.gameState.playerState.bft.FirstTurnDrawItemCount}";
		normalPanelSlotCountText.text = $"{UIBase.gameState.playerState.TableContainer.GetFinalFixedSlotCount()}";
		normalPanelDrawCountText.text = $"{UIBase.gameState.playerState.bft.turnDrawItemCount}";
	}

	private void UpdateDisplay_Battle()
	{
		base.gameObject.SetActive(value: true);
		ItemContainer tableContainer = UIBase.gameState.playerState.TableContainer;
		int finalFixedSlotCount = tableContainer.GetFinalFixedSlotCount();
		int itemsCount = tableContainer.ItemsCount;
		PlayerBftUIDisplay.Config playerBftUIDisplayConfig = UIBase.gameState.applicationState.vfxDataPreset.playerBftUIDisplayConfig;
		if (itemsCount <= finalFixedSlotCount)
		{
			glow.color = playerBftUIDisplayConfig.slotCountIdleColor;
		}
		else
		{
			glow.color = playerBftUIDisplayConfig.slotCountOverflowColor;
		}
		battlePanelSlotCountText.text = $"{itemsCount}/{finalFixedSlotCount}";
		slotCount = itemsCount;
		fixedSlotCount = finalFixedSlotCount;
	}

	private void UpdateDisplay_Info()
	{
		infoPanelFirstDrawCountText.text = $"{UIBase.gameState.playerState.bft.FirstTurnDrawItemCount}";
		infoPanelSlotCountText.text = $"{UIBase.gameState.playerState.TableContainer.GetFinalFixedSlotCount()}";
		infoPanelDrawCountText.text = $"{UIBase.gameState.playerState.bft.turnDrawItemCount}";
	}

	private void ToNormal()
	{
		if (inBattle)
		{
			inBattle = false;
			normalPanel.SetActive(value: true);
			battlePanel.SetActive(value: false);
			SetDirty();
		}
	}

	private void ToBattle()
	{
		if (!inBattle)
		{
			inBattle = true;
			normalPanel.SetActive(value: false);
			battlePanel.SetActive(value: true);
			SetDirty();
		}
	}

	private void _OnButtonClicked()
	{
		if (showInfo)
		{
			HideInfo();
		}
		else
		{
			ShowInfo();
		}
	}

	private void _OnEmptyClicked()
	{
		HideInfo();
	}

	private void _OnBattleStart(GameEvent.Type _t, GameEvent _e)
	{
		HideInfo();
		ToBattle();
	}

	private void _OnBattleEnd(GameEvent.Type _t, GameEvent _e)
	{
		HideInfo();
		ToNormal();
	}

	private void _OnActionQueueCleanEvent()
	{
		SetDirty();
	}
}
