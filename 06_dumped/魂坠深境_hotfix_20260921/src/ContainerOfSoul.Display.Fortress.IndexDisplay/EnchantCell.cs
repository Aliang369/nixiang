using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class EnchantCell : CellUI<EnchantDef>
{
	[SerializeField]
	private Image enchantIcon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[NonSerialized]
	public EnchantDef enchantDef;

	[NonSerialized]
	private EnchantIndexPanel enchantPanel;

	protected override void Init()
	{
		enchantPanel = (EnchantIndexPanel)parent;
	}

	protected override void OnSetup(EnchantDef dailyContract)
	{
		enchantDef = dailyContract;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
	}
}
