using DG.Tweening;
using UnityEngine;

public class DeathPendingRetryPanel : UIBase
{
	[SerializeField]
	public CanvasGroup retryCanvasGroup;

	[SerializeField]
	private GameButton retryContinuePanel_retryConfirmButton;

	[SerializeField]
	private GameButton retryContinuePanel_retryGiveUpButton;

	private DeathPendingPanel deathPendingPanel => parent as DeathPendingPanel;

	protected override void Init()
	{
		retryContinuePanel_retryConfirmButton.onClick.AddListener(_OnWatchADRevive);
		retryContinuePanel_retryGiveUpButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
			SetDirty();
		});
	}

	private void _OnWatchADRevive()
	{
		Bootstrap.instance.adsManager.ShowAd("revive", delegate
		{
			base.gameObject.SetActive(value: false);
			deathPendingPanel.RevivePlayerFromDeath();
		}, delegate
		{
			UIBase.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_ad_not_finished_to_get_reward);
		}, delegate
		{
			UIBase.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_ad_not_finished_to_get_reward);
		});
	}

	public Sequence PlayShowRetryPanelSeq()
	{
		Bootstrap.instance.adsManager.EnterAdScene("revive");
		Sequence sequence = DOTween.Sequence();
		sequence.AppendCallback(delegate
		{
			((Component)(object)retryCanvasGroup).gameObject.SetActive(value: true);
		});
		sequence.Append(DOTweenModuleUI.DOFade(retryCanvasGroup, 0f, 0f));
		sequence.Append(DOTweenModuleUI.DOFade(retryCanvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.deathRattlePanelFadeTime));
		sequence.Play();
		return sequence;
	}

	public Sequence PlayHideRetryPanelSeq()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(retryCanvasGroup, 1f, 0f));
		sequence.Append(DOTweenModuleUI.DOFade(retryCanvasGroup, 0f, ScriptableObjectSingleton<VFXDataPreset>.instance.deathRattlePanelFadeTime));
		sequence.AppendCallback(delegate
		{
			((Component)(object)retryCanvasGroup).gameObject.SetActive(value: false);
		});
		sequence.Play();
		return sequence;
	}

	protected override void UpdateDisplay()
	{
		retryContinuePanel_retryConfirmButton.interactable = Bootstrap.instance.adsManager.CanClaimReward("revive");
	}
}
