using System;
using TMPro;

public class ChoiceSelectCell : CellUI<SelectChoiceInfo.SelectChoice>
{
	public GameButton confirmButton;

	public TextMeshProUGUI choiceText;

	[NonSerialized]
	private SelectChoiceInfo.SelectChoice choice;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private new ChoiceSelectPanel parent;

	protected override void OnSetup(SelectChoiceInfo.SelectChoice t)
	{
		choice = t;
		textLinkObjectDisplayer.SetGenerator(t.generator);
		if (t.selectable != null)
		{
			confirmButton.interactable = t.selectable();
		}
		else
		{
			confirmButton.interactable = true;
		}
		SetDirty();
	}

	protected override void OnUnset()
	{
		choice = null;
		SetDirty();
	}

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<SelectChoiceInfo.SelectChoice>.applicationState, choiceText, base.rectTransform);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		parent = GetUIInParents<ChoiceSelectPanel>();
	}

	protected override void Dispose()
	{
		confirmButton.onClick.RemoveListener(_OnConfirmButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		choiceText.text = choice.choiceText;
	}

	private void _OnConfirmButtonClicked()
	{
		parent.ConfirmSelectChoice(choice);
	}
}
