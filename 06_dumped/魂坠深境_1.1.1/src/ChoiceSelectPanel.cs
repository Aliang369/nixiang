using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class ChoiceSelectPanel : UIBase
{
	public TextMeshProUGUI panelHint;

	public ChoiceSelectCell choiceSelectCellPrefab;

	public RectTransform choiceSelectCellParent;

	[NonSerialized]
	private CellUIPrefabPool<ChoiceSelectCell, SelectChoiceInfo.SelectChoice> choiceSelectCellPool;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	public SelectChoiceInfo info;

	public void Setup(SelectChoiceInfo t)
	{
		info = t;
		textLinkObjectDisplayer.SetGenerator(t.generator);
		SetActiveAndDirty(active: true);
	}

	public void Unset()
	{
		info = null;
		SetActiveAndDirty(active: false);
	}

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, panelHint);
		choiceSelectCellPool = GetPrefabPool<ChoiceSelectCell, SelectChoiceInfo.SelectChoice>(choiceSelectCellPrefab, choiceSelectCellParent);
	}

	protected override void UpdateDisplay()
	{
		if (info == null)
		{
			return;
		}
		panelHint.text = info.panelHint;
		choiceSelectCellPool.DespawnAll();
		foreach (SelectChoiceInfo.SelectChoice choice in info.choices)
		{
			choiceSelectCellPool.Spawn(choice);
		}
	}

	public void ConfirmSelectChoice(SelectChoiceInfo.SelectChoice choice)
	{
		choice.onSelect?.Invoke();
		info.onConfirm?.Invoke();
		Unset();
	}
}
