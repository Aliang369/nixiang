using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class EnchantModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private EnchantDisplay enchantPrefab;

	[SerializeField]
	private VerticalLayoutGroup enchantParent;

	[NonSerialized]
	private CellUIPrefabPool<EnchantDisplay, Enchant> enchantPool;

	public override void Init()
	{
		enchantPrefab.gameObject.SetActive(value: false);
		enchantPool = base.mainItemInfoDisplay.GetPrefabPool<EnchantDisplay, Enchant>(enchantPrefab, enchantParent.transform);
		CellUIPrefabPool<EnchantDisplay, Enchant> cellUIPrefabPool = enchantPool;
		cellUIPrefabPool.onInit = (Action<EnchantDisplay>)Delegate.Combine(cellUIPrefabPool.onInit, (Action<EnchantDisplay>)delegate(EnchantDisplay display)
		{
			display.GetComponent<GameButton>().onClick.AddListener(delegate
			{
				base.mainItemInfoDisplay.itemInfoDisplay.GetModule<ItemInfoReferenceModule>().ToggleSideReference();
			});
		});
	}

	public override void UpdateDisplay()
	{
		SetupEnchant();
	}

	private void SetupEnchant()
	{
		List<Enchant> list = base.item.Enchants.ToList();
		list.RemoveAll((Enchant e) => e == null);
		enchantPool.Sync(list);
		enchantParent.AutoFitLayoutSpacing();
		if (base.item.TryGetActiveOverrideSpellCostDisplay(UIDisplayModule.applicationState, base.mainItemInfoDisplay.itemInfoDisplay.triggerType, out var sourceEnchant, out var _, out var overrideCost))
		{
			enchantPool.GetActiveElement(sourceEnchant)?.SetOverrideSpellCostDisplay(active: true, overrideCost, base.item.GetCostTextColor());
		}
	}
}
