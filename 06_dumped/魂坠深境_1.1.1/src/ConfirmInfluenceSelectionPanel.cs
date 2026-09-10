using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class ConfirmInfluenceSelectionPanel : UIBase
{
	[SerializeField]
	private ConfirmInfluenceSelector selectorPrefab;

	[SerializeField]
	private RectTransform selectorParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private PickBonusSelectionPanelBase panel;

	[NonSerialized]
	private bool finishButtonPressed;

	[NonSerialized]
	private AddInfluenceBonus info;

	[NonSerialized]
	private CellUIPrefabPool<ConfirmInfluenceSelector, Influence> selectorPool;

	protected override void Init()
	{
		selectorPool = GetPrefabPool<ConfirmInfluenceSelector, Influence>(selectorPrefab, selectorParent);
		selectorPrefab.gameObject.SetActive(value: false);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
	}

	public void Setup(AddInfluenceBonus info)
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
	}

	public void SelectInfluence(ConfirmInfluenceSelector influenceSelector)
	{
		info.selectedInfluence = influenceSelector.influence;
		info.TriggerBonus(UIBase.gameState, null);
		panel.OnSubPanelFinish(info);
		Unset();
	}

	private void _OnCancelButtonClicked()
	{
		Unset();
	}
}
