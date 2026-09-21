using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttachCharacterClassModule : SkillInfoDisplay.DisplayModule
{
	[SerializeField]
	private GameObject area;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image textIcon;

	[SerializeField]
	private TextMeshProUGUI text;

	private SkillDef skillDef => base.gameSkill?.skillDef;

	public override void Setup()
	{
		area.gameObject.SetActive(value: false);
		if (!(skillDef.attachCharacterClass == null))
		{
			area.gameObject.SetActive(value: true);
			textIcon.sprite = skillDef.attachCharacterClass.professionIcon;
			text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_class_skill.FormatWith(skillDef.attachCharacterClass.characterName);
		}
	}
}
