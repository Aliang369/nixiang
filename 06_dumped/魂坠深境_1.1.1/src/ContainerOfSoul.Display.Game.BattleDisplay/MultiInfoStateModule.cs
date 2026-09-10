using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class MultiInfoStateModule : ItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private Sequence sequence;

	[NonSerialized]
	private NFXPlayState nfx;

	[NonSerialized]
	public bool suppressShowVfx;

	public override void Setup()
	{
		base.rectTransform.anchoredPosition = Vector2.zero;
		base.rectTransform.rotation = Quaternion.Euler(Vector3.zero);
		base.rectTransform.localScale = Vector3.one;
		if (!suppressShowVfx)
		{
			RectTransform backgroundVFXPivot = (base.itemInfoDisplay as ItemInfoDisplay_Normal).mainItemInfoDisplay.backgroundVFXPivot;
			nfx = UIDisplayModule.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_multiInfoDisplayPanel_ShowItem, backgroundVFXPivot, destroyOnStop: false);
		}
	}

	public override void Unset()
	{
		nfx?.Stop();
		nfx = null;
	}

	public IEnumerator OnHide()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		RectTransform target = this.RectTransform();
		ItemInfoDisplayBase.Config itemInfoDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemInfoDisplayConfig;
		sequence.Append(target.DOMove(UIDisplayModule.applicationState.runningGameState.UIManager.battlePanel.deckButton.transform.position, itemInfoDisplayConfig.enemyShowInfoShowTime));
		sequence.Join(target.DORotate(itemInfoDisplayConfig.rotateAnimStartShowRotation, itemInfoDisplayConfig.enemyShowInfoShowTime));
		sequence.Join(target.DOScale(itemInfoDisplayConfig.rotateAnimStartShowScale, itemInfoDisplayConfig.enemyShowInfoShowTime));
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
