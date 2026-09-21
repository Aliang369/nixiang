using TMPro;
using UnityEngine;

public class ItemDisplayArea : ContainerItemArea
{
	[SerializeField]
	private TextMeshProUGUI numText;

	[SerializeField]
	private RectTransform content;

	protected override void Init()
	{
		base.Init();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
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

	private void UpdateItemNumText()
	{
		numText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(GetAllItemDisplays().Count);
	}

	private void _OnEmptyClicked()
	{
		SetDirty();
	}
}
