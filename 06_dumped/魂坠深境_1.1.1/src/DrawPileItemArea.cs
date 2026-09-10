using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DrawPileItemArea : PileItemArea
{
	[SerializeField]
	private Image backButtonImage;

	[SerializeField]
	private RectTransform basePanel;

	[SerializeField]
	private Image background;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private bool showing;

	protected override void Init()
	{
		base.Init();
		manager.HideUI(this);
		backButtonImage.color = backButtonImage.color.SetA(0f);
		backButtonImage.transform.rotation = Quaternion.Euler(Vector3.zero);
		basePanel.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		showing = false;
	}

	public override void ShowPanel()
	{
		UIBase.gameState.StartCoroutine(OnShowPanel);
	}

	public override void HidePanel()
	{
		UIBase.gameState.StartCoroutine(OnHidePanel);
	}

	protected override void ShowInfo(Item item)
	{
		UIBase.gameState.UIManager.battleItemInfoPanel.Show(item, PivotManager.PivotType.PlayerDrawPilePivot);
	}

	private IEnumerator OnShowPanel()
	{
		if (!showing)
		{
			showing = true;
			base.gameObject.SetActive(value: true);
			SetDirty();
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
			AnimConfig pileItemAreaAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.pileItemAreaAnimConfig;
			sequence.Append(DOTweenModuleUI.DOFade(backButtonImage, 1f, pileItemAreaAnimConfig.fadeTime));
			sequence.Join(backButtonImage.transform.DORotate(new Vector3(0f, 0f, -20f), pileItemAreaAnimConfig.fadeTime));
			sequence.Join(basePanel.DOScale(1f, pileItemAreaAnimConfig.fadeTime));
			sequence.Join(DOTweenModuleUI.DOFade(background, 0.5f, 0.2f));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			yield return sequence.WaitForKill();
		}
	}

	private IEnumerator OnHidePanel()
	{
		if (showing)
		{
			showing = false;
			AnimConfig pileItemAreaAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.pileItemAreaAnimConfig;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
			sequence.Append(DOTweenModuleUI.DOFade(backButtonImage, 0f, pileItemAreaAnimConfig.fadeTime));
			sequence.Join(backButtonImage.transform.DORotate(Vector3.zero, pileItemAreaAnimConfig.fadeTime));
			sequence.Join(basePanel.DOScale(0.1f, pileItemAreaAnimConfig.fadeTime));
			sequence.Join(DOTweenModuleUI.DOFade(background, 0f, 0.2f));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
				manager.HideUI(this);
			});
			sequence.Play();
			yield return sequence.WaitForKill();
		}
	}
}
