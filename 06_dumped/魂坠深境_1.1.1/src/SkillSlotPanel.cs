using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using UnityEngine;

public class SkillSlotPanel : UIBase, ISelectablePanel
{
	[SerializeField]
	private SkillSlotDisplay skillSlotDisplayPrefab;

	[SerializeField]
	private bool autoSelect;

	[NonSerialized]
	public SkillSlotDisplay selected;

	[NonSerialized]
	public Action<SkillSlotDisplay> onSelectedChanged;

	[NonSerialized]
	private CellUIPrefabPool<SkillSlotDisplay, GameSkillSlot> pool;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		pool = GetPrefabPool<SkillSlotDisplay, GameSkillSlot>(skillSlotDisplayPrefab, skillSlotDisplayPrefab.transform.parent);
		if (parent is DeckPanel deckPanel)
		{
			pool.ConfigInfoPanel(deckPanel.deckInfoPanel);
		}
		else if (parent is SelectSkillActivatePanel selectSkillActivatePanel)
		{
			pool.ConfigInfoPanel(selectSkillActivatePanel.infoPanel);
		}
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		List<GameSkillSlot> list = UIBase.gameState.playerState.AllSkillsSlot.ToList();
		if (list.Count == pool.activeCount)
		{
			return;
		}
		pool.DespawnAll();
		foreach (GameSkillSlot item in list)
		{
			pool.Spawn(item);
		}
		SkillPanel.SetSideSkillPosition(pool.GetActiveElements());
		if (autoSelect)
		{
			SelectFirstEmptySlot();
		}
	}

	public void SelectFirstEmptySlot()
	{
		List<SkillSlotDisplay> list = pool.GetActiveElements() as List<SkillSlotDisplay>;
		if (list.Count == 0)
		{
			return;
		}
		selected = list[0];
		foreach (SkillSlotDisplay item in list)
		{
			if (item.skillSlot.gameSkill == null)
			{
				selected = item;
				break;
			}
		}
		SetDirty();
		onSelectedChanged?.Invoke(selected);
	}

	public List<SkillSlotDisplay> GetAiSelectableSlotDisplays()
	{
		return pool.GetActiveElements().Where(delegate(SkillSlotDisplay display)
		{
			if (display != null)
			{
				GameSkillSlot skillSlot = display.skillSlot;
				if (skillSlot != null)
				{
					return !skillSlot.isTemp;
				}
				return false;
			}
			return false;
		}).ToList();
	}

	public void SelectForAiSession(SkillSlotDisplay display)
	{
		if (!(display == null) && display.skillSlot != null && !display.skillSlot.isTemp)
		{
			selected = display;
			SetDirty();
			onSelectedChanged?.Invoke(selected);
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		SkillSlotDisplay skillSlotDisplay = module.parent as SkillSlotDisplay;
		selected = skillSlotDisplay;
		if (parent is Common.UI.UIBase uIBase)
		{
			uIBase.SetDirty();
		}
		onSelectedChanged?.Invoke(selected);
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		if (parent is DeckPanel deckPanel)
		{
			selected = null;
			deckPanel.UnselectAll();
		}
		if (parent is Common.UI.UIBase uIBase)
		{
			uIBase.SetDirty();
		}
		onSelectedChanged?.Invoke(selected);
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		SkillSlotDisplay skillSlotDisplay = module.parent as SkillSlotDisplay;
		return selected == skillSlotDisplay;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		return !(module.parent as SkillSlotDisplay).skillSlot.isTemp;
	}
}
