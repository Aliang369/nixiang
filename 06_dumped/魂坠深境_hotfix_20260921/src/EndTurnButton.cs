using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndTurnButton : UIBase
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private RectTransform movementRoot;

	private bool pushed;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
		SetDirty();
	}

	protected override void Dispose()
	{
	}

	private void _OnButtonClicked()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!pushed && UIBase.gameState.battleState.TryRequestEndPlayerTurn())
		{
			pushed = true;
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_turn_over);
		}
	}

	public IEnumerator PlayerTurnStart()
	{
		pushed = false;
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		RectTransform rectTransform = ((movementRoot != null) ? movementRoot : (base.transform as RectTransform));
		PlayerBftUIDisplay.Config playerBftUIDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig;
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(rectTransform, 0f, playerBftUIDisplayConfig.HUDShowTime, false));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayerTurnEnd()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		RectTransform rectTransform = ((movementRoot != null) ? movementRoot : (base.transform as RectTransform));
		PlayerBftUIDisplay.Config playerBftUIDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig;
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(rectTransform, rectTransform.GetWidth(), playerBftUIDisplayConfig.HUDShowTime, false));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}
}
