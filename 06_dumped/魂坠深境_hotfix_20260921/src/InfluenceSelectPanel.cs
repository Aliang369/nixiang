using System;
using Common.UI;
using UnityEngine;

public class InfluenceSelectPanel : ElementSelectPanel<Influence>
{
	[SerializeField]
	private InfluenceDisplay influenceDisplay;

	[SerializeField]
	private InfluenceDisplay influenceInfoDisplay;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> pool;

	[NonSerialized]
	private RewardUncollectedTag influenceInfoUncollectedTag;

	protected override void Init()
	{
		base.Init();
		InitUI(influenceInfoDisplay);
		influenceDisplay.gameObject.SetActive(value: false);
		influenceInfoDisplay.gameObject.SetActive(value: false);
		pool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplay, influenceDisplay.transform.parent);
		CellUIPrefabPool<InfluenceDisplay, Influence> cellUIPrefabPool = pool;
		cellUIPrefabPool.onSetup = (Action<InfluenceDisplay>)Delegate.Combine(cellUIPrefabPool.onSetup, new Action<InfluenceDisplay>(_OnInfluenceDisplaySetup));
		influenceInfoUncollectedTag = RewardUncollectedTag.AttachTo(influenceInfoDisplay, RewardUncollectedTagLayout.InfluenceSelect);
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
		influenceInfoDisplay.Unset();
		influenceInfoUncollectedTag?.Hide();
	}

	protected override Influence GetElement(SelectableModuleBase selectableModule)
	{
		return (selectableModule.parent as InfluenceDisplay).influence;
	}

	protected override void Spawn(Influence influence)
	{
		pool.Spawn(influence);
	}

	protected override void DespawnAll()
	{
		pool.DespawnAll();
	}

	protected override void OnSelect(Influence influence)
	{
		influenceInfoDisplay.Setup(influence);
		influenceInfoUncollectedTag?.Setup(influence?.def, UIBase.applicationState);
	}

	protected override void OnUnselect(Influence influence)
	{
		influenceInfoDisplay.Unset();
		influenceInfoUncollectedTag?.Hide();
	}

	private void _OnEmptyClicked()
	{
		influenceInfoDisplay.Unset();
		influenceInfoUncollectedTag?.Hide();
	}

	private void _OnInfluenceDisplaySetup(InfluenceDisplay display)
	{
		RewardUncollectedTag.AttachTo(display, RewardUncollectedTagLayout.InfluenceSelect)?.Setup(display.influence?.def, UIBase.applicationState);
	}
}
