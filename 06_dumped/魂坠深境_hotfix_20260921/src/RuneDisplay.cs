using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuneDisplay : CellUI<Rune>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		public RuneDisplay display => parent as RuneDisplay;

		public Rune rune => display.rune;
	}

	[SerializeField]
	public Image icon;

	[SerializeField]
	private bool discoverDisplay;

	[SerializeField]
	private TextMeshProUGUI enchantValue;

	[NonSerialized]
	public Rune rune;

	public bool DisplayNotDiscovered
	{
		get
		{
			if (discoverDisplay)
			{
				return !rune.def.IsDiscovered(CellUI<Rune>.applicationState);
			}
			return false;
		}
	}

	protected override void OnSetup(Rune dailyContract)
	{
		rune = dailyContract;
		base.gameObject.SetActive(value: true);
		RefreshBaseDisplay();
	}

	protected override void OnUnset()
	{
		rune = null;
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		RefreshBaseDisplay();
	}

	private void RefreshBaseDisplay()
	{
		icon.sprite = rune.def.Icon;
		icon.color = (DisplayNotDiscovered ? ColorHelper.IndexUnDiscoverColor : Color.white);
		if (!rune.def.stacking || DisplayNotDiscovered)
		{
			enchantValue.gameObject.SetActive(value: false);
			return;
		}
		enchantValue.gameObject.SetActive(value: true);
		enchantValue.text = $"{rune.value}";
	}
}
