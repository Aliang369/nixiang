using System;
using TMPro;

public class SkillWordDisplay : CellUI<SkillWord>
{
	public TextMeshProUGUI wordDescriptionText;

	[NonSerialized]
	protected TextLinkObjectDisplayer textLinkObjectDisplayer;

	private SkillWord word;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<SkillWord>.applicationState, wordDescriptionText, base.rectTransform);
	}

	protected override void OnSetup(SkillWord t)
	{
		word = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (word != null)
		{
			wordDescriptionText.text = word.GetFormatDescription(CellUI<SkillWord>.applicationState, null, out var generator);
			textLinkObjectDisplayer.SetGenerator(generator);
		}
	}
}
