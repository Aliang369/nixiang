using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyUnlockStage : UIBase, IStackPanel
{
	[Serializable]
	public class DifficultyRankSetup
	{
		public DifficultyLevel difficultyLevel;

		public GameObject vfxCenter;

		public GameObject rankUnlock;

		public Image clavisImageComponent;

		public ClavisDef attachClavis;
	}

	public CanvasGroup stageCanvasGroup;

	public List<DifficultyRankSetup> difficultyRankSetups = new List<DifficultyRankSetup>();

	public GameObject burstVFXGameObject;

	public CanvasGroup difficultyHintCanvasGroup;

	public TextMeshProUGUI difficultyHintText;

	public GameButton exitButton;

	private bool toExit;

	protected override void Init()
	{
		exitButton.onClick.AddListener(_OnExitButtonClicked);
	}

	public IEnumerator TriggerDifficultyAnim(DifficultyLevel unlockDifficulty)
	{
		toExit = false;
		((Component)(object)difficultyHintCanvasGroup).gameObject.SetActive(value: false);
		exitButton.gameObject.SetActive(value: false);
		burstVFXGameObject.gameObject.SetActive(value: false);
		stageCanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: true);
		foreach (DifficultyRankSetup difficultyRankSetup in difficultyRankSetups)
		{
			if (!UIBase.applicationState.difficultyManager.IsDifficultyFinished(difficultyRankSetup.difficultyLevel))
			{
				_ = difficultyRankSetup.difficultyLevel <= DifficultyLevel.Normal;
			}
			bool flag = UIBase.applicationState.clavisManager.IsUnlocked(difficultyRankSetup.attachClavis);
			difficultyRankSetup.rankUnlock.SetActive(flag && difficultyRankSetup.difficultyLevel != unlockDifficulty);
			if (difficultyRankSetup.attachClavis != null)
			{
				difficultyRankSetup.clavisImageComponent.gameObject.SetActive(value: true);
				difficultyRankSetup.clavisImageComponent.sprite = difficultyRankSetup.attachClavis.icon;
			}
			else
			{
				difficultyRankSetup.clavisImageComponent.gameObject.SetActive(value: false);
			}
		}
		yield return DOTweenModuleUI.DOFade(stageCanvasGroup, 1f, 0.25f).WaitForCompletion();
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_unlock_difficulty_clavis);
		DifficultyInfo difficultyInfo = UIBase.applicationState.difficultyManager.GetDifficultyInfo(unlockDifficulty);
		DifficultyRankSetup unlockSetup = difficultyRankSetups.Find((DifficultyRankSetup o) => o.difficultyLevel == unlockDifficulty);
		burstVFXGameObject.transform.position = unlockSetup.rankUnlock.transform.position;
		burstVFXGameObject.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(1.5f);
		unlockSetup.rankUnlock.SetActive(value: true);
		yield return new WaitForSeconds(1.5f);
		difficultyHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_difficulty_arg0_unlocked.FormatWith(difficultyInfo.difficultyName);
		((Component)(object)difficultyHintCanvasGroup).gameObject.SetActive(value: true);
		difficultyHintCanvasGroup.alpha = 0f;
		yield return DOTweenModuleUI.DOFade(difficultyHintCanvasGroup, 1f, 0.5f).WaitForCompletion();
		exitButton.gameObject.SetActive(value: true);
		yield return new WaitUntil(() => toExit);
		yield return DOTweenModuleUI.DOFade(stageCanvasGroup, 0f, 0.25f).WaitForCompletion();
		toExit = false;
		base.gameObject.SetActive(value: false);
	}

	private void _OnExitButtonClicked()
	{
		toExit = true;
	}
}
