using System;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using UnityEngine;
using UnityEngine.UI;

public class SkillWordModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image skillWordIconPrefab;

	[SerializeField]
	private RectTransform skillWordIconParent;

	[NonSerialized]
	public Action<SkillDisplay> onClick;

	[NonSerialized]
	private PrefabPool<Image> skillWordIconPool;

	[NonSerialized]
	private ProfileSkillPresetSlotCell slotCellParent;

	public override void Init()
	{
		slotCellParent = parent.GetUIInParents<ProfileSkillPresetSlotCell>();
		InputModule module = parent.GetModule<InputModule>();
		if (module != null)
		{
			module.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(module.onPointerClick, (Action<InputModuleBase>)delegate
			{
				if (base.profileSkill != null && !base.profileSkill.inspected)
				{
					base.profileSkill.inspected = true;
				}
				onClick?.Invoke(base.display);
			});
		}
		skillWordIconPool = new PrefabPool<Image>(skillWordIconPrefab, skillWordIconParent);
	}

	public override void UpdateDisplay()
	{
		GameSkill gameSkill = null;
		if (base.gameSkill != null)
		{
			gameSkill = base.gameSkill;
		}
		else
		{
			if (slotCellParent?.skillSlot?.skill == null)
			{
				return;
			}
			gameSkill = slotCellParent.skillSlot.GetGameSkill();
		}
		icon.sprite = gameSkill.skillDef.Icon;
		skillWordIconPool.DespawnAll();
	}

	public override void Unset()
	{
		onClick = null;
	}
}
