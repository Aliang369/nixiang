using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class InfluenceMultiInfoPanelModule : InfluenceDisplay.DisplayModule
{
	[NonSerialized]
	private Sequence sequence;

	public override void Setup()
	{
		base.gameObject.SetActive(value: true);
		base.transform.localScale = Vector3.one;
	}

	public IEnumerator OnHide()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
		float hideMoveTime = ScriptableObjectSingleton<VFXDataPreset>.instance.multiInfoPanelDisplayConfig.hideMoveTime;
		sequence.Append(this.RectTransform().DOMove(UIDisplayModule.applicationState.runningGameState.UIManager.battlePanel.influencePanel.transform.position, hideMoveTime));
		sequence.Join(this.RectTransform().DOScale(Vector3.zero, hideMoveTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}
}
