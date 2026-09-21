using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillReplaceConfirmPanel : UIBase
{
	[SerializeField]
	private SkillDisplay skillDisplay_BeReplace;

	[SerializeField]
	private SkillDisplay skillDisplay_NewOne;

	[SerializeField]
	private TextMeshProUGUI descriptionText_2;

	[SerializeField]
	private GameButton yesButton;

	[SerializeField]
	private GameButton noButton;

	[SerializeField]
	private Toggle toggle;

	[NonSerialized]
	private Action onConfirm;

	protected override void Init()
	{
		InitUI(skillDisplay_BeReplace);
		InitUI(skillDisplay_NewOne);
		base.gameObject.SetActive(value: false);
		yesButton.onClick.AddListener(delegate
		{
			onConfirm?.Invoke();
			Unset();
		});
		noButton.onClick.AddListener(Unset);
		toggle.onValueChanged.AddListener(delegate(bool v)
		{
			UIBase.gameState.showSkillReplaceConfirm = !v;
		});
	}

	public void Setup(GameSkill skill_BeReplace, GameSkill skill_NewOne, Action onConfirm)
	{
		this.onConfirm = onConfirm;
		toggle.SetIsOnWithoutNotify(!UIBase.gameState.showSkillReplaceConfirm);
		base.gameObject.SetActive(value: true);
		skillDisplay_BeReplace.Setup(skill_BeReplace);
		skillDisplay_NewOne.Setup(skill_NewOne);
		descriptionText_2.text = base.textData.term_replace_skill.FormatWith(skill_BeReplace.skillDef.skillName, skill_NewOne.skillDef.skillName);
	}

	public void Unset()
	{
		base.gameObject.SetActive(value: false);
		onConfirm = null;
	}
}
