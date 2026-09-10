using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GameOverPanel : UIBase
{
	public const float panelFadeTime = 0.5f;

	[SerializeField]
	private CharacterDisplay characterDisplay;

	[SerializeField]
	public GameOverRewardPanel gameOverRewardPanel;

	[SerializeField]
	public GameOverBriefPanel gameOverBriefPanel;

	[SerializeField]
	public LevelUpDisplay levelUpDisplay;

	[SerializeField]
	public MultyLevelUpDisplay multyLevelUpDisplay;

	[SerializeField]
	private GameButton toUnlockPanelButton;

	[SerializeField]
	private GameButton backToMenuButton;

	[NonSerialized]
	private WrapperEnumerator backToMenuCoroutine;

	[NonSerialized]
	private GameRecord record;

	[NonSerialized]
	private WrapperEnumerator toUnlockPanelCoroutine;

	[NonSerialized]
	private bool shareCaptureControlsHidden;

	[NonSerialized]
	private bool shareButtonWasActive;

	[NonSerialized]
	private bool toUnlockPanelButtonWasActive;

	[NonSerialized]
	private int shareCaptureRevision;

	public void Setup(GameRecord record)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		RestoreShareCaptureControls();
		base.gameObject.SetActive(value: true);
		this.record = record;
		gameOverRewardPanel.Setup(record);
		gameOverRewardPanel.SetActiveAndDirty(active: false);
		gameOverBriefPanel.Setup(record);
		gameOverBriefPanel.SetActiveAndDirty(active: true);
		Bootstrap.instance.audioManager.StopAllSustained();
		Bootstrap.instance.audioManager.PlaySustained(record.success ? ScriptableObjectSingleton<AudioData>.instance.bgm_gameover_win : ScriptableObjectSingleton<AudioData>.instance.bgm_gameover_lose, "BgmTag");
	}

	protected override void Init()
	{
		InitUI(characterDisplay);
		InitUI(gameOverRewardPanel);
		InitUI(gameOverBriefPanel);
		InitUI(levelUpDisplay);
		InitUI(multyLevelUpDisplay);
		backToMenuButton.onClick.AddListener(_OnBackToMenuButtonClicked);
		toUnlockPanelButton.onClick.AddListener(_OnToUnlockPanelButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnUIEmptyClickedEvent);
	}

	protected override void Dispose()
	{
		RestoreShareCaptureControls();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnUIEmptyClickedEvent);
	}

	public void BeginShareCapture()
	{
		if (!shareCaptureControlsHidden)
		{
			shareButtonWasActive = gameOverBriefPanel.shareButton.gameObject.activeSelf;
			toUnlockPanelButtonWasActive = toUnlockPanelButton.gameObject.activeSelf;
			int revision = ++shareCaptureRevision;
			UIBase.gameState.StartCoroutine(() => RestoreShareCaptureControlsNextFrame(revision));
			shareCaptureControlsHidden = true;
			gameOverBriefPanel.shareButton.gameObject.SetActive(value: false);
			toUnlockPanelButton.gameObject.SetActive(value: false);
		}
	}

	public void RestoreShareCaptureControls()
	{
		if (shareCaptureControlsHidden)
		{
			shareCaptureControlsHidden = false;
			shareCaptureRevision++;
			toUnlockPanelButton.gameObject.SetActive(toUnlockPanelButtonWasActive);
			gameOverBriefPanel.shareButton.gameObject.SetActive(shareButtonWasActive);
		}
	}

	private IEnumerator RestoreShareCaptureControlsNextFrame(int revision)
	{
		yield return null;
		if (shareCaptureControlsHidden && revision == shareCaptureRevision)
		{
			RestoreShareCaptureControls();
		}
	}

	private IEnumerator ToUnlockPanelCor()
	{
		gameOverBriefPanel.detailInterval = 0.05f;
		yield return new WaitForSeconds(0.5f);
		gameOverRewardPanel.gameObject.SetActive(value: true);
		UIBase.gameState.StartCoroutine(() => gameOverRewardPanel.ShowRewardsCor());
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(gameOverRewardPanel.canvasGroup, 0f, 0f));
		sequence.Append(DOTweenModuleUI.DOFade(gameOverBriefPanel.canvasGroup, 0f, 0.5f));
		sequence.Append(DOTweenModuleUI.DOFade(gameOverRewardPanel.canvasGroup, 1f, 0.5f));
		sequence.AppendCallback(delegate
		{
			gameOverBriefPanel.gameObject.SetActive(value: false);
		});
		yield return sequence.Play().WaitForCompletion();
	}

	private IEnumerator BackToMenuCoroutine()
	{
		UIBase.applicationState.playerInfoManager.showUnlockTalent = true;
		gameOverRewardPanel.cellIntervalTime = 0.05f;
		yield return new WaitUntil(() => gameOverRewardPanel.finished);
		Bootstrap.instance.audioManager.StopAllSustained();
		UIBase.applicationState.savingManager.SaveProfile();
		Bootstrap.instance.Load(BootStrapLoadData.LoadToFortressAfterGameRecord(Bootstrap.instance.CurrentAccountID, record));
	}

	public void HideAll()
	{
		UIBase.gameState.UIManager.universalCommonInfoPanel.HideAll();
	}

	private void _OnToUnlockPanelButtonClicked()
	{
		if (toUnlockPanelCoroutine == null)
		{
			toUnlockPanelCoroutine = UIBase.gameState.StartCoroutine(ToUnlockPanelCor);
			HideAll();
		}
	}

	private void _OnBackToMenuButtonClicked()
	{
		if (backToMenuCoroutine == null)
		{
			backToMenuCoroutine = UIBase.gameState.StartCoroutine(BackToMenuCoroutine);
			HideAll();
		}
	}

	private void _OnUIEmptyClickedEvent()
	{
		HideAll();
	}
}
