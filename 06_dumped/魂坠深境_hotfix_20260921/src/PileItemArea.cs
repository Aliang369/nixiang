using System;
using TMPro;
using UnityEngine;

public abstract class PileItemArea : ContainerItemArea, ISelectablePanel
{
	[Serializable]
	public class AnimConfig
	{
		[SerializeField]
		public float fadeTime;
	}

	[SerializeField]
	private GameButton backgroundButton;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI numText;

	[SerializeField]
	private RectTransform content;

	[NonSerialized]
	private Item selectedItem;

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selectedItem == item;
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selectedItem == item)
		{
			Unselect();
		}
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		return true;
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		selectedItem = item;
		ShowInfo(selectedItem);
		SetDirty();
	}

	protected override void Init()
	{
		base.Init();
		backgroundButton.onClick.AddListener(_OnBackButtonClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		selectedItem = null;
		Vector2 anchoredPosition = content.anchoredPosition;
		anchoredPosition.y = 0f;
		content.anchoredPosition = anchoredPosition;
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		UpdateItemNumText();
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	public abstract void ShowPanel();

	public abstract void HidePanel();

	public void Unselect()
	{
		selectedItem = null;
		UIBase.gameState.UIManager.battleItemInfoPanel.Hide();
		SetDirty();
	}

	protected abstract void ShowInfo(Item item);

	private void UpdateItemNumText()
	{
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(GetAllItemDisplays().Count);
	}

	private void _OnEmptyClicked()
	{
		Unselect();
	}

	private void _OnBackButtonClicked()
	{
		HidePanel();
	}
}
