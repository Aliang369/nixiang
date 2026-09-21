using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class ItemSkinShuffleInfoPanel : UIBase, IFrameUpdate, IShowAsync
{
	[SerializeField]
	private GameButton rotationRuleButton;

	[SerializeField]
	private GameObject rotationRulePanel;

	[SerializeField]
	private GameButton rotationRulePanelCloseButton;

	[SerializeField]
	private TextMeshProUGUI remainTimeText;

	[SerializeField]
	private GameObject remainTimeArea;

	[SerializeField]
	private CellUIPrefabPoolConfig<SkinSetBatchCell, ItemSkinSetBatch> skinBatchPool;

	private List<ItemSkinSetBatch> shuffleList => ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseShuffle;

	private int currentPeriod => ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekPeriod;

	protected override void Init()
	{
		Init(skinBatchPool);
		rotationRuleButton.onClick.AddListener(_OnOpenRotationRuleButtonClicked);
		rotationRulePanelCloseButton.onClick.AddListener(_OnCloseRotationRulePanelButtonClicked);
		rotationRulePanel.SetActive(value: false);
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		rotationRulePanel.SetActive(value: false);
		UpdateDisplay();
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			UpdateRemainTime();
		}
	}

	private void UpdateRemainTime()
	{
		if (shuffleList == null || shuffleList.Count == 0)
		{
			remainTimeArea.SetActive(value: false);
			return;
		}
		remainTimeArea.SetActive(value: true);
		UnityDateTime endTime = UIBase.applicationState.seasonManager.currentSeason.def.endTime;
		TimeSpan span = endTime.UtcDateTime - DateTime.UtcNow;
		remainTimeText.text = TextDisplayHelper.GetTimeSpanText(span);
	}

	protected override void UpdateDisplay()
	{
		skinBatchPool.DespawnAll();
		if (shuffleList == null || shuffleList.Count == 0)
		{
			remainTimeArea.SetActive(value: false);
			return;
		}
		skinBatchPool.Sync(shuffleList);
		UpdateRemainTime();
	}

	private void _OnOpenRotationRuleButtonClicked()
	{
		rotationRulePanel.SetActive(value: true);
	}

	private void _OnCloseRotationRulePanelButtonClicked()
	{
		rotationRulePanel.SetActive(value: false);
	}
}
