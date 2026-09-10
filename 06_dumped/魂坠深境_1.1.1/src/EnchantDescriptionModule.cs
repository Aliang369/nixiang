using System;
using TMPro;
using UnityEngine;

public class EnchantDescriptionModule : EnchantDisplay.DisplayModule
{
	[SerializeField]
	protected TextMeshProUGUI descriptionText;

	[NonSerialized]
	protected TextLinkObjectDisplayer textLinkObjectDisplayer;

	public override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIDisplayModule.applicationState, descriptionText, base.rectTransform);
	}

	public override void Setup()
	{
		if (base.display.DisplayNotDiscovered)
		{
			descriptionText.text = "";
			return;
		}
		descriptionText.text = base.enchant.GetDescription(UIDisplayModule.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
	}
}
