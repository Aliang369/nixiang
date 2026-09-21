using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ChallengeHintPanel : UIBase
{
	private const float fadeTime = 0.5f;

	private const float stayTime = 1f;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private Queue<Achievement> toShowAchievements;

	protected override void Init()
	{
		toShowAchievements = new Queue<Achievement>();
		((Component)(object)canvasGroup).gameObject.SetActive(value: false);
		UIBase.gameState.applicationState.eventManager.achievementAchieveEvent.AddListener(_AchieveAchievement);
	}

	protected override void UpdateDisplay()
	{
		if (sequence != null)
		{
			return;
		}
		if (toShowAchievements.Count == 0)
		{
			((Component)(object)canvasGroup).gameObject.SetActive(value: false);
			return;
		}
		Achievement achievement = toShowAchievements.Dequeue();
		titleText.text = achievement.achievementDef.achievementName;
		descriptionText.text = achievement.GetDescription(UIBase.applicationState, out var _);
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0f));
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.5f));
		sequence.AppendInterval(1f);
		sequence.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.5f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
			SetDirty();
		});
		sequence.Play();
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.achievementAchieveEvent.RemoveListener(_AchieveAchievement);
	}

	private void _AchieveAchievement(Achievement achievement)
	{
		toShowAchievements.Enqueue(achievement);
		((Component)(object)canvasGroup).gameObject.SetActive(value: true);
		SetDirty();
	}
}
