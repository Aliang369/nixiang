using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class SkillSlotDisplay : CellUI<GameSkillSlot>, IDisplayModulesController
{
	[SerializeField]
	public SkillDisplay skillDisplay;

	[SerializeField]
	public TextMeshProUGUI text;

	[SerializeField]
	public GameObject emptySlotDisplay;

	[NonSerialized]
	public GameSkillSlot skillSlot;

	protected override void Init()
	{
		InitUI(skillDisplay);
	}

	protected override void OnSetup(GameSkillSlot skillSlot)
	{
		this.skillSlot = skillSlot;
	}

	protected override void UpdateDisplay()
	{
		if (skillSlot.gameSkill == null)
		{
			emptySlotDisplay.gameObject.SetActive(value: true);
			text.gameObject.SetActive(value: true);
			skillDisplay.gameObject.SetActive(value: false);
		}
		else
		{
			emptySlotDisplay.gameObject.SetActive(value: false);
			text.gameObject.SetActive(value: false);
			skillDisplay.gameObject.SetActive(value: true);
			skillDisplay.Setup(skillSlot.gameSkill);
		}
	}
}
