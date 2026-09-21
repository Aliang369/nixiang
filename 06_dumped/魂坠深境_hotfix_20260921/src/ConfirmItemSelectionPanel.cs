using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmItemSelectionPanel : UIBase
{
	[SerializeField]
	private ConfirmItemSelector selectorPrefab;

	[SerializeField]
	private RectTransform selectorParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Text unselectCountText;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private PickBonusSelectionPanelBase panel;

	[SerializeField]
	private GameButton recommendPanelButton;

	[NonSerialized]
	private SelectGetItemBonus bonus;

	[NonSerialized]
	private bool finishButtonPressed;

	[NonSerialized]
	public List<ConfirmItemSelector> selected;

	[NonSerialized]
	private CellUIPrefabPool<ConfirmItemSelector, Item> selectorPool;

	public int UnselectedCount
	{
		get
		{
			if (bonus != null)
			{
				return SelectGetItemBonus.maxSelectCount - selected.Count;
			}
			return 0;
		}
	}

	protected override void Init()
	{
		selectorPool = GetPrefabPool<ConfirmItemSelector, Item>(selectorPrefab, selectorParent);
		selectorPool.ConfigInfoPanel(panel.itemInfoPanel);
		selected = new List<ConfirmItemSelector>();
		selectorPrefab.gameObject.SetActive(value: false);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		recommendPanelButton.onClick.AddListener(delegate
		{
			UIBase.gameState.UIManager.recommendItemPanel.ShowRecommend(UIBase.gameState.playerState.character.def.attachClass);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "其他-卡组推荐界面", "卡牌选择");
		});
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	protected override void UpdateDisplay()
	{
		unselectCountText.text = UnselectedCount.ToString();
		confirmButton.gameObject.SetActive(selected.Count != 0);
	}

	public void Setup(SelectGetItemBonus bonus)
	{
		UIBase.gameState.applicationState.eventManager.showItemBonusSelectionPanelEvent.TriggerEvent(bonus);
		base.gameObject.SetActive(value: true);
		finishButtonPressed = false;
		this.bonus = bonus;
		titleText.text = bonus.previewText;
		selectorPool.DespawnAll();
		foreach (Item allItem in bonus.allItems)
		{
			selectorPool.Spawn(allItem);
		}
		cancelButton.gameObject.SetActive(bonus.minSelectCount < 1);
		SetDirty();
	}

	public void Unset()
	{
		base.gameObject.SetActive(value: false);
		selectorPool.DespawnAll();
		selected.Clear();
		panel.itemInfoPanel.HideAllCommonInfo();
	}

	public void Select(ConfirmItemSelector selector)
	{
		if (SelectGetItemBonus.maxSelectCount == 1)
		{
			TriggerItemSelect_Single(selector);
		}
		else
		{
			if (SelectGetItemBonus.maxSelectCount < 1)
			{
				throw new Exception($"info.maxSelectCount error: {SelectGetItemBonus.maxSelectCount}");
			}
			TriggerItemSelect_Multiple(selector);
		}
		SetDirty();
	}

	private void TriggerItemSelect_Single(ConfirmItemSelector selector)
	{
		if (selected.Contains(selector))
		{
			selected.Remove(selector);
		}
		else
		{
			selected.Clear();
			selected.Add(selector);
		}
		SetDirty();
	}

	private void TriggerItemSelect_Multiple(ConfirmItemSelector selector)
	{
		if (selected.Contains(selector))
		{
			selected.Remove(selector);
		}
		else if (selected.Count < SelectGetItemBonus.maxSelectCount)
		{
			selected.Add(selector);
		}
		else
		{
			selected.RemoveAt(0);
			selected.Add(selector);
		}
		SetDirty();
	}

	private void FinishItemSelection()
	{
		if (!finishButtonPressed)
		{
			finishButtonPressed = true;
			List<Item> list = new List<Item>();
			for (int i = 0; i < selected.Count; i++)
			{
				list.Add(selected[i].item);
			}
			bonus.selectItems = list;
			bonus.TriggerBonus(UIBase.gameState, null);
			panel.OnSubPanelFinish(bonus);
		}
	}

	private void _OnConfirmButtonClicked()
	{
		FinishItemSelection();
		Unset();
		panel.itemInfoPanel.HideAllCommonInfo();
	}

	private void _OnCancelButtonClicked()
	{
		Unset();
	}

	private void _OnEmptyClicked()
	{
		selected.Clear();
		SetDirty();
	}
}
