using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.SkillDisplayModule;

public class CooldownModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private Image coolDown;

	[SerializeField]
	private TextMeshProUGUI coolDownNum;

	public override void UpdateDisplay()
	{
		if (base.gameSkill.isPassiveSkill || base.gameSkill.CoolDowned)
		{
			coolDown.gameObject.SetActive(value: false);
			coolDownNum.gameObject.SetActive(value: false);
		}
		else
		{
			coolDown.gameObject.SetActive(value: true);
			coolDownNum.gameObject.SetActive(value: true);
			coolDownNum.text = $"{base.gameSkill.cooldown}";
		}
	}
}
