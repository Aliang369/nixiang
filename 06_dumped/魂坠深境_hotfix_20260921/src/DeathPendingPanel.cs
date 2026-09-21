using System;
using System.Collections;
using DG.Tweening;
using FMOD;
using UnityEngine;

public class DeathPendingPanel : UIBase
{
	private const float retryReviveHealthPercent = 1f;

	[SerializeField]
	private CanvasGroup deathSentenceCanvasGroup;

	[SerializeField]
	private GameButton retryConfirmButton;

	[SerializeField]
	private GameButton retryGiveUpButton;

	[SerializeField]
	private GameObject retryWatchAdHint;

	[SerializeField]
	private DeathPendingRetryPanel retryContinuePanel;

	[NonSerialized]
	private bool deathSentenced;

	[NonSerialized]
	private bool isGiveUp;

	[NonSerialized]
	private bool isGiveUpFinished;

	[NonSerialized]
	public bool pending;

	[NonSerialized]
	public Action deathCallback;

	[NonSerialized]
	public GUID? currentBGM;

	private bool NeedWatchAdToRevive
	{
		get
		{
			if (!UIBase.applicationState.playerInfoManager.vip)
			{
				return !Application.isEditor;
			}
			return false;
		}
	}

	protected override void Init()
	{
		InitUI(retryContinuePanel);
		pending = false;
		deathSentenced = false;
		isGiveUp = false;
		isGiveUpFinished = false;
		retryConfirmButton.onClick.AddListener(delegate
		{
			retryContinuePanel.gameObject.SetActive(value: true);
		});
		retryGiveUpButton.onClick.AddListener(_OnGiveUp);
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		retryContinuePanel.gameObject.SetActive(value: false);
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	public void TriggerDeathPending(Action onDeath)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (!pending)
		{
			pending = true;
			deathSentenceCanvasGroup.alpha = 0f;
			deathCallback = onDeath;
			if (!UIBase.gameState.applicationState.guideManager.skipDeathPendingFinalPanel)
			{
				UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.DeathPending);
			}
			currentBGM = Bootstrap.instance.audioManager.GetSustainedEventGUID("BgmTag");
			Bootstrap.instance.audioManager.PlaySustained(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_death_pending, "BgmTag");
			if (UIBase.gameState.playerState.deathPendingReviveUsed || UIBase.gameState.applicationState.guideManager.skipDeathPendingFinalPanel)
			{
				UIBase.gameState.StartCoroutine(DirectDeathCor);
			}
			else
			{
				UIBase.gameState.StartCoroutine(ShowPendingPanelCor);
			}
		}
	}

	private IEnumerator ShowPendingPanelCor()
	{
		isGiveUp = false;
		UIBase.gameState.playerState.deathPendingReviveUsed = true;
		UIBase.gameState.UIManager.gameVirtualCamera.DoToDeathRattle();
		yield return new WaitUntil(() => !UIBase.gameState.UIManager.gameVirtualCamera.isTweening);
		base.gameObject.SetActive(value: true);
		retryWatchAdHint.SetActive(NeedWatchAdToRevive);
		Sequence t = retryContinuePanel.PlayShowRetryPanelSeq();
		yield return t.WaitForCompletion();
	}

	private IEnumerator ReviveCor()
	{
		pending = false;
		retryContinuePanel.PlayHideRetryPanelSeq();
		UIBase.gameState.UIManager.gameVirtualCamera.DoOutDeathRattle();
		yield return new WaitUntil(() => !UIBase.gameState.UIManager.gameVirtualCamera.isTweening);
		Bootstrap.instance.audioManager.StopSustained("BgmTag");
		if (currentBGM.HasValue)
		{
			Bootstrap.instance.audioManager.PlaySustained(currentBGM.Value, "BgmTag");
		}
		base.gameObject.SetActive(value: false);
	}

	public void RevivePlayerFromDeath()
	{
		if (pending)
		{
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.DeathPending);
			UIBase.gameState.UIManager.StartCoroutine(ReviveCor());
			int targetValue = Mathf.Max((int)((float)(int)UIBase.gameState.playerState.bft.maxHealth * 1f), 1);
			UIBase.gameState.actionManager.EnqueueAction(new SetBftHealthAction(targetValue, null, UIBase.gameState.playerState.bft));
		}
	}

	private IEnumerator DirectDeathCor()
	{
		isGiveUp = false;
		base.gameObject.SetActive(value: true);
		retryContinuePanel.retryCanvasGroup.alpha = 0f;
		UIBase.gameState.UIManager.playerItemArea.OnBattleEnd();
		UIBase.gameState.UIManager.StartCoroutine(UIBase.gameState.UIManager.battlePanel.HidePanel());
		VirtualCamera virtualCamera = UIBase.gameState.UIManager.gameVirtualCamera;
		virtualCamera.DoToDeathRattle();
		yield return new WaitUntil(() => !virtualCamera.isTweening);
		yield return DeathDropCor();
	}

	private IEnumerator PendingToDeathCor()
	{
		pending = false;
		UIBase.gameState.UIManager.playerItemArea.OnBattleEnd();
		UIBase.gameState.UIManager.StartCoroutine(UIBase.gameState.UIManager.battlePanel.HidePanel());
		Sequence t = retryContinuePanel.PlayHideRetryPanelSeq();
		yield return t.WaitForCompletion();
		yield return DeathDropCor();
	}

	private IEnumerator DeathDropCor()
	{
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_death_stinger);
		UIBase.gameState.gameEventManager.TriggerPlayerDeath();
		VirtualCamera virtualCamera = UIBase.gameState.UIManager.gameVirtualCamera;
		virtualCamera.DoDeathDrop();
		yield return DOTweenModuleUI.DOFade(UIBase.gameState.UIManager.basePanelsRoot, 0f, ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.darkenTime).WaitForCompletion();
		yield return new WaitUntil(() => !virtualCamera.isTweening);
		if (UIBase.gameState.applicationState.guideManager.skipDeathPendingFinalPanel)
		{
			isGiveUpFinished = true;
			DeathEndGame();
		}
		else
		{
			Sequence t = PlayDeathSentenceSeq();
			yield return t.WaitForCompletion();
			isGiveUpFinished = true;
		}
	}

	private Sequence PlayDeathSentenceSeq()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.AppendCallback(delegate
		{
			((Component)(object)deathSentenceCanvasGroup).gameObject.SetActive(value: true);
		});
		sequence.Append(DOTweenModuleUI.DOFade(deathSentenceCanvasGroup, 0f, 0f));
		sequence.Append(DOTweenModuleUI.DOFade(deathSentenceCanvasGroup, 1f, ScriptableObjectSingleton<VFXDataPreset>.instance.deathRattlePanelFadeTime));
		sequence.Play();
		return sequence;
	}

	private void DeathEndGame()
	{
		deathSentenced = true;
		UIBase.gameState.actionManager.Reset();
		UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.DeathPending);
		base.gameObject.SetActive(value: false);
		Bootstrap.instance.audioManager.StopSustained("BgmTag");
		deathCallback?.Invoke();
	}

	private void _OnGiveUp()
	{
		isGiveUp = true;
		UIBase.gameState.StartCoroutine(PendingToDeathCor);
	}

	private void _OnEmptyAreaClicked()
	{
		if (isGiveUpFinished && !deathSentenced)
		{
			DeathEndGame();
		}
	}
}
