using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class GameConfigDisplaySkillCell : CellUI<SkillDef>
{
	[SerializeField]
	public Image icon;

	[SerializeField]
	public TextMeshProUGUI nameText;

	[NonSerialized]
	public SkillDef skill;

	protected override void OnSetup(SkillDef t)
	{
		skill = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (skill != null)
		{
			nameText.text = skill.skillName;
			icon.sprite = skill.Icon;
		}
	}
}
