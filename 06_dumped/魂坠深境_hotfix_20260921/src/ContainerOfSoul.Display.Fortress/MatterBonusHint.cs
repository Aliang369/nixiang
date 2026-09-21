using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress;

public class MatterBonusHint : UIBase
{
	public Image matterIconImage;

	public TextMeshProUGUI matterCountText;

	public CanvasGroup canvasGroup;

	private MatterInfo matterInfo;

	private Sequence seq;

	public void Setup(MatterInfo t)
	{
		matterInfo = t;
		SetDirty();
	}

	public void Unset()
	{
	}

	public void Hint()
	{
		if (seq == null && matterInfo != null)
		{
			seq = DOTween.Sequence();
			seq.AppendCallback(delegate
			{
				matterIconImage.gameObject.SetActive(value: true);
				matterIconImage.sprite = matterInfo.matterDef.icon;
				matterCountText.text = matterInfo.count.ToString();
				canvasGroup.alpha = 0f;
				base.gameObject.SetActive(value: true);
			});
			seq.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.2f));
			seq.AppendInterval(0.8f);
			seq.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.2f));
			seq.AppendCallback(delegate
			{
				base.gameObject.SetActive(value: false);
				seq = null;
			});
			seq.Play();
		}
	}
}
