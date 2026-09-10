using System;
using TMPro;
using UnityEngine;

public class InfluenceTextModule : InfluenceDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI influenceDescriptionText;

	[NonSerialized]
	protected TextLinkObjectDisplayer textLinkObjectDisplayer;

	public override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIDisplayModule.applicationState, influenceDescriptionText, base.rectTransform);
	}

	public override void Setup()
	{
		if (base.display.DisplayNotDiscovered)
		{
			influenceDescriptionText.text = "";
			return;
		}
		influenceDescriptionText.text = base.influence.GetInfluenceDescription(UIDisplayModule.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
	}
}
