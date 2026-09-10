using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DarkScreenPanel : UIBase, IPointerClickHandler, IEventSystemHandler
{
	public CanvasGroup canvasGroup;

	public TextMeshProUGUI centerText;

	public Tween tween;

	public bool Tweening
	{
		get
		{
			if (tween != null && tween.IsActive())
			{
				return !tween.IsComplete();
			}
			return false;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		tween.Complete(withCallbacks: true);
	}

	public void Show()
	{
		if (Tweening)
		{
			tween.Complete(withCallbacks: true);
		}
		centerText.text = "";
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0f));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: true);
		});
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.darkScreenFadeTime));
		sequence.Play();
		tween = sequence;
	}

	public void Hide()
	{
		if (Tweening)
		{
			tween.Complete(withCallbacks: true);
		}
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0f));
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, ScriptableObjectSingleton<VFXDataPreset>.instance.darkScreenFadeTime));
		sequence.AppendCallback(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		tween = sequence;
	}

	public void ShotText(string text)
	{
		if (Tweening)
		{
			tween.Complete(withCallbacks: true);
		}
		centerText.text = text;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(centerText.DOFade(0f, 0f));
		sequence.AppendCallback(delegate
		{
			centerText.gameObject.SetActive(value: true);
		});
		sequence.Append(centerText.DOFade(1f, ScriptableObjectSingleton<VFXDataPreset>.instance.darkScreenTextFadeTime));
		sequence.AppendInterval(ScriptableObjectSingleton<VFXDataPreset>.instance.darkScreenTextStayTime);
		sequence.Append(centerText.DOFade(0f, ScriptableObjectSingleton<VFXDataPreset>.instance.darkScreenTextFadeTime));
		sequence.AppendCallback(delegate
		{
			centerText.gameObject.SetActive(value: true);
		});
		sequence.Play();
		tween = sequence;
	}
}
