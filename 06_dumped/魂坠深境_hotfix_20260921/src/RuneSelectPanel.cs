using System;
using Common.UI;
using UnityEngine;

public class RuneSelectPanel : ElementSelectPanel<Rune>
{
	[SerializeField]
	private RuneDisplay runeDisplay;

	[SerializeField]
	private RuneDisplay runeInfoDisplay;

	[NonSerialized]
	private CellUIPrefabPool<RuneDisplay, Rune> pool;

	protected override void Init()
	{
		base.Init();
		InitUI(runeInfoDisplay);
		runeDisplay.gameObject.SetActive(value: false);
		runeInfoDisplay.gameObject.SetActive(value: false);
		pool = GetPrefabPool<RuneDisplay, Rune>(runeDisplay, runeDisplay.transform.parent);
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
		runeInfoDisplay.Unset();
	}

	protected override Rune GetElement(SelectableModuleBase selectableModule)
	{
		return (selectableModule.parent as RuneDisplay).rune;
	}

	protected override void Spawn(Rune rune)
	{
		pool.Spawn(rune);
	}

	protected override void DespawnAll()
	{
		pool.DespawnAll();
	}

	protected override void OnSelect(Rune rune)
	{
		runeInfoDisplay.Setup(rune);
	}

	protected override void OnUnselect(Rune rune)
	{
		runeInfoDisplay.Unset();
	}

	private void _OnEmptyClicked()
	{
		runeInfoDisplay.Unset();
	}
}
