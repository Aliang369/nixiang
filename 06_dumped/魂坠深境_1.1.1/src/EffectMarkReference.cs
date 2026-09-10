using System;
using TMPro;
using UnityEngine;

public class EffectMarkReference : CellUI<EffectMarkAbility.EffectMarkInfo>
{
	[SerializeField]
	protected EffectMarkDisplay displayIcon;

	[SerializeField]
	protected TextMeshProUGUI descriptionText;

	[SerializeField]
	protected TextMeshProUGUI sourceText;

	[NonSerialized]
	private EffectMarkAbility.EffectMarkInfo info;

	protected override void Init()
	{
		base.Init();
		InitUI(displayIcon);
	}

	protected override void OnSetup(EffectMarkAbility.EffectMarkInfo t)
	{
		info = t;
		displayIcon.Setup(t);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (info != null)
		{
			descriptionText.text = info.GetDescriptionText();
			sourceText.text = info.GetSourceText();
		}
	}
}
