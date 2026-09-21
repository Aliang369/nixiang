using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.SkillDisplayModule;

public class DurabilityModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI durabilityText;

	[SerializeField]
	private Image durabilityIcon;

	public override void UpdateDisplay()
	{
		try
		{
			durabilityIcon.gameObject.SetActive(base.gameSkill.WithDurability);
			if (base.gameSkill.DurabilityType == SkillDef.SkillPropertyType.CastDurability)
			{
				durabilityIcon.color = Color.white;
			}
			else if (base.gameSkill.DurabilityType == SkillDef.SkillPropertyType.BattleDurability)
			{
				durabilityIcon.color = Color.yellow;
			}
			durabilityText.text = $"{base.gameSkill.durability}";
		}
		catch (Exception ex)
		{
			TraceLog.Error("[DurabilityModule] UpdateDisplay() \n- [" + base.gameSkill?.skillDef?.skillName + "] \n- " + base.transform.FullPath() + " \n- " + ex.Message);
		}
	}
}
