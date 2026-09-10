using System;
using Common.UI;
using UnityEngine;

public class SkillSelectPanel : ElementSelectPanel<GameSkill>
{
	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> pool;

	protected override void Init()
	{
		base.Init();
		InitUI(skillInfoDisplay);
		skillDisplay.gameObject.SetActive(value: false);
		skillInfoDisplay.gameObject.SetActive(value: false);
		pool = GetPrefabPool<SkillDisplay, GameSkill>(skillDisplay, skillDisplay.transform.parent);
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
		skillInfoDisplay.Unset();
	}

	protected override GameSkill GetElement(SelectableModuleBase selectableModule)
	{
		return (selectableModule.parent as SkillDisplay)?.setupData;
	}

	protected override void Spawn(GameSkill GameSkill)
	{
		pool.Spawn(GameSkill);
	}

	protected override void DespawnAll()
	{
		pool.DespawnAll();
	}

	protected override void OnSelect(GameSkill GameSkill)
	{
		skillInfoDisplay.Setup(GameSkill);
	}

	protected override void OnUnselect(GameSkill GameSkill)
	{
		skillInfoDisplay.Unset();
	}

	private void _OnEmptyClicked()
	{
		skillInfoDisplay.Unset();
	}
}
