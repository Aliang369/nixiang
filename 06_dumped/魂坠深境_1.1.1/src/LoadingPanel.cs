using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
	public class LoadingShowInfo
	{
		public string loadingFinishedTitleText;

		public Sprite loadingImage;

		public Level loadingLevel;

		public string loadingTitleText;

		public string loadingHintText;

		public bool showLoadingTitleText;

		public float minDuration;
	}

	[Serializable]
	public class Config
	{
		[SerializeField]
		public float showTime;

		[SerializeField]
		public float hidePanelTime;

		[SerializeField]
		public float healthBarToTargetTime;

		[SerializeField]
		public float healthBarToFinishTime;

		[SerializeField]
		public float healthBarFadeTime;

		[SerializeField]
		public Sprite defaultLoadingImage;

		[SerializeField]
		public List<Sprite> loadingImages;
	}

	[SerializeField]
	private GameButton panelButton;

	[NonSerialized]
	public Action onPanelHide;

	[SerializeField]
	private CanvasGroup loadingPanelCanvasGroup;

	[SerializeField]
	private CanvasGroup contentCanvasGroup;

	[SerializeField]
	private Image loadingImage;

	[SerializeField]
	private CanvasGroup levelNameCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI levelNameText;

	[SerializeField]
	private TextMeshProUGUI continueText;

	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private Color loadingImageDefaultColor;

	[SerializeField]
	public RectTransform healthBarPanel;

	[SerializeField]
	public RectTransform healthBar;

	[NonSerialized]
	private float healthBarProgress;

	[NonSerialized]
	private Sequence panelSequence;

	[NonSerialized]
	private Sequence progressBarSequence;

	[NonSerialized]
	public LoadingShowInfo showInfo;

	[NonSerialized]
	public bool showing;

	[NonSerialized]
	public DateTime loadingStartTime;

	public bool LoadingFinished => healthBarProgress >= 1f;

	public void Init()
	{
		showing = false;
	}

	private void SetProgressBar(float toHealthBarProgress)
	{
		healthBarProgress = toHealthBarProgress;
		healthBar.SetWidth(healthBarPanel.GetWidth() * healthBarProgress);
	}

	private void SetupShowInfo(LoadingShowInfo showInfo)
	{
		if (showInfo.loadingImage != null)
		{
			loadingImage.sprite = showInfo.loadingImage;
		}
		if (showInfo.loadingTitleText != null)
		{
			levelNameText.text = showInfo.loadingTitleText;
		}
		if (showInfo.loadingLevel != null)
		{
			if (showInfo.loadingImage == null)
			{
				loadingImage.sprite = showInfo.loadingLevel.levelLoadingImage;
			}
			if (showInfo.loadingTitleText == null)
			{
				levelNameText.text = showInfo.loadingLevel.Name;
			}
		}
		hintText.text = this.showInfo.loadingHintText;
		((Component)(object)levelNameCanvasGroup).gameObject.SetActive(showInfo.showLoadingTitleText);
		if (ScriptableObjectSingleton<LocalizationManager>.instance != null)
		{
			SystemLanguage key = LocalizationManager.LanguageCodeToSystemLanguage(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
			LauncherLocalization val = CommonConfig.Instance.launcherLocalizations.TryGetValue(key);
			continueText.text = val?.term_press_to_continue ?? "";
		}
		else
		{
			continueText.text = CommonConfig.Instance.GetCurrentLauncherLocalization().Item2.term_press_to_continue;
		}
	}

	public void Show(LoadingShowInfo showInfo)
	{
		this.showInfo = showInfo;
		loadingStartTime = DateTime.Now;
		panelSequence?.Kill();
		panelSequence = DOTween.Sequence();
		showing = true;
		SetupShowInfo(showInfo);
		Config loadingPanelConfig = GlobalConfigManager.instance.loadingPanelConfig;
		loadingPanelCanvasGroup.alpha = 0f;
		((Component)(object)loadingPanelCanvasGroup).gameObject.SetActive(value: true);
		contentCanvasGroup.alpha = 1f;
		healthBarPanel.GetComponent<CanvasGroup>().alpha = 0f;
		healthBarPanel.gameObject.SetActive(value: true);
		continueText.alpha = 1f;
		continueText.gameObject.SetActive(value: false);
		loadingPanelCanvasGroup.interactable = false;
		SetProgressBar(0f);
		NotifyProgress(0.8f);
		panelSequence.Append(DOTweenModuleUI.DOFade(loadingPanelCanvasGroup, 1f, loadingPanelConfig.showTime));
		panelSequence.Join(DOTweenModuleUI.DOFade(healthBarPanel.GetComponent<CanvasGroup>(), 1f, loadingPanelConfig.healthBarFadeTime));
		loadingPanelCanvasGroup.blocksRaycasts = true;
		Sequence sequence = panelSequence;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			panelSequence = null;
		});
		panelSequence.Play();
	}

	public void NotifyProgress(float toHealthBarProgress)
	{
		Config loadingPanelConfig = GlobalConfigManager.instance.loadingPanelConfig;
		bool finish = toHealthBarProgress >= 1f;
		progressBarSequence?.Kill();
		progressBarSequence = DOTween.Sequence();
		float duration = loadingPanelConfig.healthBarToTargetTime;
		if (finish)
		{
			float num = Mathf.Max(showInfo.minDuration - (float)(DateTime.Now - loadingStartTime).TotalSeconds, 0f);
			duration = loadingPanelConfig.healthBarToFinishTime + num;
		}
		progressBarSequence.Append(DOTween.To(() => healthBarProgress, SetProgressBar, toHealthBarProgress, duration));
		if (finish)
		{
			progressBarSequence.AppendCallback(delegate
			{
				continueText.alpha = 0f;
				continueText.gameObject.SetActive(value: true);
			});
			progressBarSequence.Append(DOTweenModuleUI.DOFade(healthBarPanel.GetComponent<CanvasGroup>(), 0f, loadingPanelConfig.healthBarFadeTime));
			progressBarSequence.Join(continueText.DOFade(1f, loadingPanelConfig.healthBarFadeTime));
			progressBarSequence.AppendCallback(delegate
			{
				healthBarPanel.gameObject.SetActive(value: false);
				loadingPanelCanvasGroup.interactable = true;
				if (showInfo.loadingFinishedTitleText != null)
				{
					levelNameText.text = showInfo.loadingFinishedTitleText;
				}
				healthBarPanel.GetComponent<CanvasGroup>().alpha = 1f;
				HideLoadingPanel();
			});
		}
		Sequence sequence = progressBarSequence;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			progressBarSequence = null;
			if (finish)
			{
				healthBarPanel.gameObject.SetActive(value: false);
				healthBarPanel.GetComponent<CanvasGroup>().alpha = 1f;
				continueText.alpha = 1f;
				continueText.gameObject.SetActive(value: true);
				loadingPanelCanvasGroup.interactable = true;
			}
		});
		progressBarSequence.Play();
	}

	public void HideLoadingPanel()
	{
		progressBarSequence?.Kill();
		panelSequence?.Kill();
		panelSequence = DOTween.Sequence();
		onPanelHide?.Invoke();
		loadingPanelCanvasGroup.blocksRaycasts = false;
		showing = false;
		Config loadingPanelConfig = GlobalConfigManager.instance.loadingPanelConfig;
		panelSequence.Append(DOTweenModuleUI.DOFade(contentCanvasGroup, 0f, loadingPanelConfig.hidePanelTime));
		Sequence sequence = panelSequence;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			panelSequence = null;
			((Component)(object)loadingPanelCanvasGroup).gameObject.SetActive(value: false);
			loadingPanelCanvasGroup.blocksRaycasts = false;
			contentCanvasGroup.alpha = 1f;
			levelNameCanvasGroup.alpha = 1f;
		});
		panelSequence.Play();
	}
}
