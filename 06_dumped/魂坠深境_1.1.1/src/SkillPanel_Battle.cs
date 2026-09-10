using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SkillPanel_Battle : SkillPanel_Side
{
	private const int MaxExpandedSkillCount = 9;

	private const float ExpandedTop = 294.1f;

	private const float ExpandedBottom = 664.9f;

	private const float ExpandedLeft = 93f;

	private const float ExpandedRight = 213.6f;

	private static readonly Vector2[] BattleSkillCenters = new Vector2[15]
	{
		new Vector2(93f, 479.5f),
		new Vector2(93f, 391.3f),
		new Vector2(93f, 567.7f),
		new Vector2(93f, 294.1f),
		new Vector2(176.7f, 479.5f),
		new Vector2(93f, 664.9f),
		new Vector2(93f, 294.1f),
		new Vector2(213.6f, 391.3f),
		new Vector2(213.6f, 567.7f),
		new Vector2(93f, 664.9f),
		new Vector2(93f, 294.1f),
		new Vector2(213.6f, 391.3f),
		new Vector2(93f, 479.5f),
		new Vector2(213.6f, 567.7f),
		new Vector2(93f, 664.9f)
	};

	private static readonly int[] BattleSkillLayoutStarts = new int[5] { 0, 1, 3, 6, 10 };

	[SerializeField]
	public SkillInfoPanel infoPanel;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		base.Init();
		InitUI(infoPanel);
		pool.ConfigInfoPanel(infoPanel);
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		IReadOnlyList<SkillDisplay> activeElements = pool.GetActiveElements();
		if (activeElements.Count == 0 || activeElements.Count > 9)
		{
			return;
		}
		RectTransform rectTransform = base.transform as RectTransform;
		if (rectTransform == null)
		{
			return;
		}
		Rect rect = rectTransform.rect;
		if (rect.width <= 0f || rect.height <= 0f)
		{
			return;
		}
		bool flag = activeElements.Count <= BattleSkillLayoutStarts.Length;
		int num = (flag ? BattleSkillLayoutStarts[activeElements.Count - 1] : 0);
		for (int i = 0; i < activeElements.Count; i++)
		{
			RectTransform rectTransform2 = activeElements[i].rectTransform.parent as RectTransform;
			if (!(rectTransform2 == null))
			{
				Vector2 vector = (flag ? BattleSkillCenters[num + i] : new Vector2((i % 2 == 0) ? 93f : 213.6f, Mathf.Lerp(294.1f, 664.9f, (float)i / (float)(activeElements.Count - 1))));
				Vector3 position = new Vector3(rect.xMin + vector.x, rect.center.y + 540f - vector.y, 0f);
				Vector3 position2 = rectTransform.TransformPoint(position);
				activeElements[i].rectTransform.anchoredPosition = rectTransform2.InverseTransformPoint(position2);
			}
		}
	}

	public IEnumerator PlayerTurnStart()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		RectTransform rectTransform = base.transform as RectTransform;
		PlayerBftUIDisplay.Config playerBftUIDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig;
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(rectTransform, 0f, playerBftUIDisplayConfig.HUDShowTime, false));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator PlayerTurnEnd()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		RectTransform rectTransform = base.transform as RectTransform;
		PlayerBftUIDisplay.Config playerBftUIDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig;
		sequence.Append(DOTweenModuleUI.DOAnchorPosX(rectTransform, 0f - rectTransform.GetWidth(), playerBftUIDisplayConfig.HUDShowTime, false));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}
}
