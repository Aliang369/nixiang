using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class EnchantModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private VerticalLayoutGroup enchantParent;

	[SerializeField]
	private EnchantDisplay enchantPrefab;

	[NonSerialized]
	private CellUIPrefabPool<EnchantDisplay, Enchant> enchantDisplayPool;

	[NonSerialized]
	private List<EnchantDisplay> enchantDisplays;

	public override void Init()
	{
		enchantDisplayPool = base.slotDisplay.GetPrefabPool<EnchantDisplay, Enchant>(enchantPrefab, enchantParent.transform);
		enchantDisplays = new List<EnchantDisplay>();
	}

	public override void Setup()
	{
		enchantPrefab.gameObject.SetActive(value: false);
	}

	public override void UpdateDisplay()
	{
		IReadOnlyList<Enchant> enchants = base.slotDisplay.item.Enchants;
		if (enchants == null || enchants.Count == 0)
		{
			foreach (EnchantDisplay enchantDisplay in enchantDisplays)
			{
				enchantDisplayPool.Despawn(enchantDisplay);
			}
			enchantDisplays.Clear();
			return;
		}
		for (int i = 0; i < enchants.Count; i++)
		{
			if (enchantDisplays.Count <= i)
			{
				enchantDisplays.Add(enchantDisplayPool.Spawn(enchants[i]));
			}
			else
			{
				enchantDisplays[i].Setup(enchants[i]);
			}
		}
		for (int j = enchants.Count; j < enchantDisplays.Count; j++)
		{
			EnchantDisplay obj = enchantDisplays[enchants.Count];
			enchantDisplayPool.Despawn(obj);
			enchantDisplays.Remove(obj);
		}
		enchantParent.AutoFitLayoutSpacing();
		if (base.slotDisplay.item.TryGetActiveOverrideSpellCostDisplay(UIDisplayModule.applicationState, base.slotDisplay.triggerType, out var sourceEnchant, out var _, out var overrideCost))
		{
			GetEnchantDisplay(sourceEnchant)?.SetOverrideSpellCostDisplay(active: true, overrideCost, base.slotDisplay.item.GetCostTextColor());
		}
	}

	public override void Unset()
	{
		foreach (EnchantDisplay enchantDisplay in enchantDisplays)
		{
			enchantDisplayPool.Despawn(enchantDisplay);
		}
		enchantDisplays.Clear();
	}

	public EnchantDisplay GetEnchantDisplay(Enchant enchant)
	{
		foreach (EnchantDisplay enchantDisplay in enchantDisplays)
		{
			if (enchantDisplay.enchant == enchant)
			{
				return enchantDisplay;
			}
		}
		return null;
	}
}
