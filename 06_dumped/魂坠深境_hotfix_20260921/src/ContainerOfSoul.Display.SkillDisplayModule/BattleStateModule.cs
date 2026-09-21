using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.SkillDisplayModule;

public class BattleStateModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image outline;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image darken;

	[SerializeField]
	private Image coolDown;

	[SerializeField]
	private TextMeshProUGUI coolDownNum;

	[SerializeField]
	private GameButton button;

	[NonSerialized]
	private bool castable;

	[NonSerialized]
	private Sequence sequence;

	public override void Init()
	{
		button.onClick.AddListener(_OnCharacterSkillButtonClicked);
	}

	public override void Setup()
	{
		if (base.gameSkill.isPassiveSkill)
		{
			outline.gameObject.SetActive(value: false);
			darken.gameObject.SetActive(value: false);
			coolDown.gameObject.SetActive(value: false);
			coolDownNum.gameObject.SetActive(value: false);
		}
		else
		{
			castable = true;
			icon.rectTransform.localScale = Vector3.one;
			outline.gameObject.SetActive(value: true);
			base.display.SetDirty();
		}
	}

	public override void UpdateDisplay()
	{
		if (!base.gameSkill.isPassiveSkill)
		{
			bool num = !base.gameSkill.CoolDowned;
			bool flag = CanDisplayAsCastable();
			if (!num)
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
			darken.gameObject.SetActive(!flag);
			SetCastable(flag);
		}
	}

	private bool CanDisplayAsCastable()
	{
		if (!base.gameSkill.isPassiveSkill)
		{
			return base.gameSkill.IsCastAble(UIDisplayModule.gameState);
		}
		return false;
	}

	private void SetCastable(bool castable)
	{
		if (this.castable != castable)
		{
			this.castable = castable;
			outline.gameObject.SetActive(castable);
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
			float num = (castable ? 1f : 0.8f);
			sequence.Append(icon.rectTransform.DOScale(new Vector3(num, num, num), 0.8f));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	private void _OnCharacterSkillButtonClicked()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (UIDisplayModule.gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase || !UIDisplayModule.gameState.actionManager.isEmpty || !UIDisplayModule.gameState.battleState.isInBattle || (!base.gameSkill.isPassiveSkill && !base.gameSkill.IsCastAble(UIDisplayModule.gameState)))
		{
			return;
		}
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_amulet_click);
		GameEvent eve = new GameEvent(GameEvent.Type.CastSkill, GameEvent.TriggerType.Real)
		{
			gameState = UIDisplayModule.gameState,
			CasterSkill = base.gameSkill
		};
		if (Bootstrap.instance.globalSaveConfigData.quickUseSkill && !base.gameSkill.isPassiveSkill && base.gameSkill.SkillSpell.targetRequirement == null && !base.gameSkill.SkillSpell.yinYangChoice)
		{
			base.gameSkill.Cast(eve);
		}
		else if (base.gameSkill.isPassiveSkill || base.gameSkill.SkillSpell.targetRequirement == null)
		{
			UIDisplayModule.gameState.UIManager.battleSkillConfirmPanel.Setup(base.gameSkill, eve);
		}
		else
		{
			base.gameSkill.SkillSpell.targetRequirement.TriggerPrepare(eve, delegate
			{
				base.gameSkill.Cast(eve);
				base.display.SetDirty();
			});
		}
		UIDisplayModule.gameState.UIManager.playerBftUIDisplay.skillPanel.infoPanel.HideAllCommonInfo();
		UIDisplayModule.gameState.UIManager.battleItemInfoPanel.Hide();
		UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.CancelSpell();
		base.display.SetDirty();
	}
}
