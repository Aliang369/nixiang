using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillConfirmPanel : UIBase
{
	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI cooldownText;

	[SerializeField]
	private TextMeshProUGUI castTimesText;

	[SerializeField]
	private TextMeshProUGUI wordDescriptionText;

	[SerializeField]
	private TextMeshProUGUI castAbleHint;

	[SerializeField]
	private GameButton yinButton;

	[SerializeField]
	private GameButton yangButton;

	[SerializeField]
	private SkillGemArea skillGemArea;

	[NonSerialized]
	private GameEvent eve;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	public GameSkill skill;

	protected override void Init()
	{
		InitUI(skillGemArea);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, text);
		base.gameObject.SetActive(value: false);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		yinButton.onClick.AddListener(delegate
		{
			ClosePanel();
			eve.choiceNumber = 0;
			skill.Cast(eve);
		});
		yangButton.onClick.AddListener(delegate
		{
			ClosePanel();
			eve.choiceNumber = 1;
			skill.Cast(eve);
		});
	}

	public void Setup(GameSkill skill, GameEvent eve)
	{
		this.skill = skill;
		this.eve = eve;
		base.gameObject.SetActive(value: true);
		SetDirty();
		skillIcon.sprite = skill.Icon;
		skillGemArea.Setup(skill);
		if (skill.isPassiveSkill)
		{
			confirmButton.gameObject.SetActive(value: false);
			yinButton.gameObject.SetActive(value: false);
			yangButton.gameObject.SetActive(value: false);
			cooldownText.gameObject.SetActive(value: false);
		}
		else
		{
			confirmButton.gameObject.SetActive(!skill.SkillSpell.yinYangChoice);
			yinButton.gameObject.SetActive(skill.SkillSpell.yinYangChoice);
			yangButton.gameObject.SetActive(skill.SkillSpell.yinYangChoice);
			cooldownText.gameObject.SetActive(value: true);
		}
	}

	protected override void UpdateDisplay()
	{
		text.text = TextDataPreset.FormatSkillDescription(skill, UIBase.gameState, isBattleDisplay: true, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		bool flag = CanConfirmSkillCast();
		if (UIBase.gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase || !flag)
		{
			confirmButton.interactable = false;
			yinButton.interactable = false;
			yangButton.interactable = false;
		}
		else
		{
			confirmButton.interactable = true;
			yinButton.interactable = true;
			yangButton.interactable = true;
		}
		titleText.text = skill.NameWithQualityColor;
		if (!skill.isPassiveSkill)
		{
			cooldownText.text = (skill.OneCastPerBattle ? skill.GetOnCastDescription() : ScriptableObjectSingleton<TextDataPreset>.instance.term_cooldown_arg0.FormatWith(skill.CastCooldown));
		}
		castTimesText.text = skill.GetDurabilityDescription();
		wordDescriptionText.text = skill.GetSkillWordsDescription(UIBase.gameState);
		if (flag || !skill.WithSpell)
		{
			castAbleHint.gameObject.SetActive(value: false);
			return;
		}
		castAbleHint.gameObject.SetActive(value: true);
		if (!skill.CoolDowned)
		{
			castAbleHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_cooldown_unable_cast;
		}
		else
		{
			castAbleHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_unable_cast_not_check_castable;
		}
	}

	protected bool CanConfirmSkillCast()
	{
		if (skill.IsCastAble(UIBase.gameState))
		{
			return UIBase.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase;
		}
		return false;
	}

	private void ClosePanel()
	{
		base.gameObject.SetActive(value: false);
		UIBase.gameState.UIManager.playerBftUIDisplay.SetDirty();
	}

	private void _OnCancelButtonClicked()
	{
		ClosePanel();
	}

	private void _OnConfirmButtonClicked()
	{
		if (UIBase.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase && UIBase.gameState.actionManager.isEmpty && UIBase.gameState.battleState.isInBattle)
		{
			ClosePanel();
			skill.Cast(eve);
		}
	}
}
