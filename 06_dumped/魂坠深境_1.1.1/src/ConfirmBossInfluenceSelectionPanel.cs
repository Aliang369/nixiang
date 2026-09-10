using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class ConfirmBossInfluenceSelectionPanel : UIBase
{
	[SerializeField]
	private ConfirmBossInfluenceSelector selectorPrefab;

	[SerializeField]
	private RectTransform selectorParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private TextMeshProUGUI willPowerText;

	[SerializeField]
	private PickBonusSelectionPanelBase panel;

	[NonSerialized]
	private bool finishButtonPressed;

	[NonSerialized]
	private AddBossInfluenceBonus info;

	[NonSerialized]
	private CellUIPrefabPool<ConfirmBossInfluenceSelector, Influence> selectorPool;

	protected override void Init()
	{
		selectorPool = GetPrefabPool<ConfirmBossInfluenceSelector, Influence>(selectorPrefab, selectorParent);
		selectorPrefab.gameObject.SetActive(value: false);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
	}

	public void Setup(AddBossInfluenceBonus info)
	{
		this.info = info;
		finishButtonPressed = false;
		base.gameObject.SetActive(value: true);
		foreach (Influence item in info.selectableInfluence)
		{
			selectorPool.Spawn(item);
		}
		SetDirty();
	}

	public void Unset()
	{
		selectorPool.DespawnAll();
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		titleText.text = info.previewText;
		willPowerText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(UIBase.gameState.playerState.CurrentUsedWillPower, UIBase.gameState.playerState.MaxWillPower);
	}

	public void SelectInfluence(ConfirmBossInfluenceSelector influenceSelector)
	{
		Influence influence = influenceSelector.influence;
		if (!TryShowWillPowerOverLimitConfirm(influence))
		{
			ConfirmSelectInfluence(influence);
		}
	}

	private bool TryShowWillPowerOverLimitConfirm(Influence influence)
	{
		if (!UIBase.gameState.playerState.ShouldShowWillPowerOverLimitConfirm(influence.willPowerCost, out var overLimitWillPower, out var penaltyInfluences))
		{
			return false;
		}
		UIBase.gameState.UIManager.commonSecondConfirmPanel.SetupWillPowerOverLimit(new WillPowerOverLimitConfirmInfo
		{
			overLimitWillPower = overLimitWillPower,
			penaltyInfluences = penaltyInfluences,
			confirmCallback = delegate
			{
				ConfirmSelectInfluence(influence);
			}
		});
		return true;
	}

	private void ConfirmSelectInfluence(Influence influence)
	{
		info.selectedInfluence = influence;
		info.TriggerBonus(UIBase.gameState, null);
		panel.OnSubPanelFinish(info);
		Unset();
	}

	private void _OnCancelButtonClicked()
	{
		Unset();
	}
}
