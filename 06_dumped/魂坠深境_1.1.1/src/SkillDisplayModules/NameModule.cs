using TMPro;
using UnityEngine;

namespace SkillDisplayModules;

public class NameModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI skillNameText;

	public override void Setup()
	{
		skillNameText.text = base.gameSkill.NameWithQualityAndUpgrade;
	}
}
