using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class QuickHint : CellUI<QuickHint.Info>
{
	public class Info
	{
		public string s;

		public float timeScale;
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI text;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(Info info)
	{
		base.transform.SetAsFirstSibling();
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = info.timeScale;
		text.text = info.s;
		base.gameObject.SetActive(value: true);
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.battleHintPanelConfig.smallFadeTime));
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.battleHintPanelConfig.smallStayTime));
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, ScriptableObjectSingleton<VFXDataPreset>.instance.battleHintPanelConfig.smallFadeTime));
		sequence.onKill = delegate
		{
			base.gameObject.SetActive(value: false);
			sequence = null;
			(parent as QuickHintPanel).Despawn(this);
		};
		sequence.Play();
	}
}
