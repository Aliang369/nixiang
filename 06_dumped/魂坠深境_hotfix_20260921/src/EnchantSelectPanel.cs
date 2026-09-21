using System;
using Common.UI;
using UnityEngine;

public class EnchantSelectPanel : ElementSelectPanel<Enchant>
{
	[SerializeField]
	private EnchantDisplay enchantDisplay;

	[SerializeField]
	private EnchantDisplay enchantInfoDisplay;

	[NonSerialized]
	private CellUIPrefabPool<EnchantDisplay, Enchant> pool;

	protected override void Init()
	{
		base.Init();
		InitUI(enchantInfoDisplay);
		enchantDisplay.gameObject.SetActive(value: false);
		enchantInfoDisplay.gameObject.SetActive(value: false);
		pool = GetPrefabPool<EnchantDisplay, Enchant>(enchantDisplay, enchantDisplay.transform.parent);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	public override void Unset()
	{
		base.Unset();
		enchantInfoDisplay.Unset();
	}

	protected override Enchant GetElement(SelectableModuleBase selectableModule)
	{
		return (selectableModule.parent as EnchantDisplay).enchant;
	}

	protected override void Spawn(Enchant element)
	{
		pool.Spawn(element);
	}

	protected override void DespawnAll()
	{
		pool.DespawnAll();
	}

	protected override void OnSelect(Enchant enchant)
	{
		enchantInfoDisplay.Setup(enchant);
	}

	protected override void OnUnselect(Enchant enchant)
	{
		enchantInfoDisplay.Unset();
	}

	private void _OnEmptyClicked()
	{
		enchantInfoDisplay.Unset();
	}
}
