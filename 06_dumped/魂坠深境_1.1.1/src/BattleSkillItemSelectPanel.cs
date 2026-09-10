using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillItemSelectPanel : ItemSelectPanel
{
	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private TextMeshProUGUI skillName;

	[SerializeField]
	private TextMeshProUGUI cooldownText;

	[SerializeField]
	private TextMeshProUGUI castTimesText;

	[SerializeField]
	private TextMeshProUGUI wordDescriptionText;

	[SerializeField]
	private TextMeshProUGUI castAbleHint;

	[SerializeField]
	private SkillGemArea skillGemArea;

	[NonSerialized]
	private GameSkill skill;

	protected override void Init()
	{
		InitUI(skillGemArea);
		base.Init();
	}

	public override void Setup(SelectItemInfo info)
	{
		skill = info.selectSource as GameSkill;
		base.Setup(info);
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		skillGemArea.Setup(skill);
		skillIcon.sprite = skill.Icon;
		skillName.text = skill.NameWithQualityColor;
		cooldownText.text = (skill.OneCastPerBattle ? skill.GetOnCastDescription() : ScriptableObjectSingleton<TextDataPreset>.instance.term_cooldown_arg0.FormatWith(skill.CastCooldown));
		castTimesText.gameObject.SetActive(skill.WithDurability);
		castTimesText.text = skill.GetDurabilityDescription();
		wordDescriptionText.text = skill.GetSkillWordsDescription(UIBase.gameState);
		confirmText.text = TextDataPreset.FormatSkillDescription(skill, UIBase.gameState, isBattleDisplay: true, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		if (CanConfirm() || !skill.WithSpell)
		{
			castAbleHint.gameObject.SetActive(value: false);
			return;
		}
		castAbleHint.gameObject.SetActive(value: true);
		if (!skill.CoolDowned)
		{
			castAbleHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_cooldown_unable_cast;
		}
		else if (!CanConfirm())
		{
			castAbleHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_need_select_unable_cast;
		}
	}

	protected override bool CanConfirm()
	{
		if (base.CanConfirm() && skill.IsCastAble(UIBase.gameState))
		{
			return UIBase.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase;
		}
		return false;
	}
}
