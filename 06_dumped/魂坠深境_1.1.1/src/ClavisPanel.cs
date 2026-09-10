using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class ClavisPanel : UIBase
{
	[SerializeField]
	private ClavisDisplay clavisDisplayPrefab;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private TextMeshProUGUI numText;

	[SerializeField]
	private ClavisInfoPanel infoPanel;

	[NonSerialized]
	private CellUIPrefabPool<ClavisDisplay, ClavisDef> clavisDisplayPool;

	protected override void Init()
	{
		if (infoPanel != null)
		{
			InitUI(infoPanel);
		}
		clavisDisplayPool = GetPrefabPool<ClavisDisplay, ClavisDef>(clavisDisplayPrefab, clavisDisplayPrefab.transform.parent);
		if (parent is DeckPanel deckPanel)
		{
			clavisDisplayPool.ConfigInfoPanel(deckPanel.deckInfoPanel);
		}
		else
		{
			clavisDisplayPool.ConfigInfoPanel(infoPanel);
		}
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	protected override void UpdateDisplay()
	{
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(UIBase.applicationState.clavisManager.AllVisibleClavis, clavisDisplayPool, layoutGroup);
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_count_arg0.FormatWith(UIBase.applicationState.clavisManager.UnlockedVisibleClavis.Count);
	}

	private void _OnEmptyClicked()
	{
		SetDirty();
	}
}
