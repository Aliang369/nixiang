using DG.Tweening;
using TMPro;
using UnityEngine;

public class SubtitlePanel : UIBase
{
	public TextMeshProUGUI subtitleText;

	public CanvasGroup canvasGroup;

	private Tween fadeTween;

	public bool Tweening
	{
		get
		{
			if (fadeTween != null && fadeTween.IsActive())
			{
				return !fadeTween.IsComplete();
			}
			return false;
		}
	}

	public void ShowSubtitle(string text)
	{
		if (Tweening)
		{
			fadeTween.Complete();
		}
		subtitleText.text = text;
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0f));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: true);
		});
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.subtitleFadeTime));
		sequence.Play();
		fadeTween = sequence;
	}

	public void HideSubtitle()
	{
		if (Tweening)
		{
			fadeTween.Complete();
		}
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0f));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: true);
		});
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, ScriptableObjectSingleton<VFXDataPreset>.instance.subtitleFadeTime));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		fadeTween = sequence;
	}

	public void ShotSubtitle(string text, float stayTime)
	{
		if (Tweening)
		{
			fadeTween.Complete();
		}
		subtitleText.text = text;
		Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0f));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: true);
		});
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.subtitleFadeTime));
		sequence.AppendInterval(stayTime);
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, ScriptableObjectSingleton<VFXDataPreset>.instance.subtitleFadeTime));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		fadeTween = sequence;
	}
}
