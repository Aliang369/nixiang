using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnchantDisplay : CellUI<Enchant>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		public EnchantDisplay display => parent as EnchantDisplay;

		public Enchant enchant => display.enchant;
	}

	[SerializeField]
	private Image enchantIcon;

	[SerializeField]
	private bool discoverDisplay;

	[SerializeField]
	private TextMeshProUGUI enchantValue;

	[SerializeField]
	private GameObject overrideCostDisplay;

	[SerializeField]
	private TextMeshProUGUI overrideCostText;

	[SerializeField]
	private CanvasGroup overrideCostCanvasGroup;

	[NonSerialized]
	public Enchant enchant;

	[NonSerialized]
	private bool overrideCostDisplayActive;

	[NonSerialized]
	private int overrideCostDisplayCost;

	[NonSerialized]
	private Color overrideCostDisplayTextColor = Color.white;

	public bool DisplayNotDiscovered
	{
		get
		{
			if (discoverDisplay)
			{
				return !enchant.def.IsDiscovered(CellUI<Enchant>.applicationState);
			}
			return false;
		}
	}

	protected override void OnSetup(Enchant enchant)
	{
		this.enchant = enchant;
		ClearOverrideSpellCostDisplayState();
		base.gameObject.SetActive(value: true);
		UpdateDisplay();
	}

	protected override void OnUnset()
	{
		base.OnUnset();
		ClearOverrideSpellCostDisplayState();
		ApplyOverrideSpellCostDisplay();
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		enchantIcon.sprite = enchant.def.Icon;
		if (DisplayNotDiscovered || !enchant.def.stacking)
		{
			enchantValue.gameObject.SetActive(value: false);
		}
		else
		{
			enchantValue.gameObject.SetActive(value: true);
			enchantValue.text = $"{enchant.value}";
		}
		enchantIcon.color = (DisplayNotDiscovered ? ColorHelper.IndexUnDiscoverColor : Color.white);
		ApplyOverrideSpellCostDisplay();
	}

	public void SetOverrideSpellCostDisplay(bool active, int cost, Color costTextColor)
	{
		overrideCostDisplayActive = active;
		overrideCostDisplayCost = cost;
		overrideCostDisplayTextColor = costTextColor;
		ApplyOverrideSpellCostDisplay();
	}

	private void ClearOverrideSpellCostDisplayState()
	{
		overrideCostDisplayActive = false;
		overrideCostDisplayCost = 0;
		overrideCostDisplayTextColor = Color.white;
	}

	private void ApplyOverrideSpellCostDisplay()
	{
		EnsureOverrideCostDisplayReferences();
		if (overrideCostDisplay == null)
		{
			return;
		}
		if (overrideCostText == null)
		{
			overrideCostDisplay.SetActive(value: false);
			return;
		}
		overrideCostDisplay.SetActive(overrideCostDisplayActive);
		if (overrideCostDisplayActive)
		{
			overrideCostText.text = TextDisplayHelper.ToNumSpriteString(overrideCostDisplayCost);
			overrideCostText.color = overrideCostDisplayTextColor;
			if ((UnityEngine.Object)(object)overrideCostCanvasGroup != null)
			{
				overrideCostCanvasGroup.alpha = 0.75f;
			}
		}
	}

	private void EnsureOverrideCostDisplayReferences()
	{
		if (!(overrideCostDisplay != null) || !(overrideCostText != null))
		{
			Transform transform = base.transform.Find("OverrideCostDisplay");
			if (!(transform == null))
			{
				overrideCostDisplay = transform.gameObject;
				overrideCostText = transform.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
				overrideCostCanvasGroup = transform.GetComponent<CanvasGroup>();
			}
		}
	}
}
