using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TrigramsStateDisplay : UIBase
{
	private class TextDisplayInfo
	{
		public Sequence sequence;

		public bool showing;

		public readonly TextMeshProUGUI text;

		public TextDisplayInfo(TextMeshProUGUI text)
		{
			this.text = text;
			showing = false;
		}
	}

	[SerializeField]
	private TextMeshProUGUI extremeYinText;

	[SerializeField]
	private TextMeshProUGUI extremeYangText;

	[SerializeField]
	private TextMeshProUGUI balanceText;

	[NonSerialized]
	private TrigramsManager.YinYangStateType currentState;

	[NonSerialized]
	private TrigramsManager.YinYangStateType imageDisplayConfigType;

	[NonSerialized]
	private Sequence imageSequence;

	[NonSerialized]
	private List<TextDisplayInfo> textDisplayInfos;

	protected override void Init()
	{
		textDisplayInfos = new List<TextDisplayInfo>();
		textDisplayInfos.Add(new TextDisplayInfo(extremeYinText));
		textDisplayInfos.Add(new TextDisplayInfo(extremeYangText));
		textDisplayInfos.Add(new TextDisplayInfo(balanceText));
		foreach (TextDisplayInfo textDisplayInfo in textDisplayInfos)
		{
			textDisplayInfo.showing = false;
			textDisplayInfo.text.alpha = 0f;
		}
		extremeYinText.gameObject.SetActive(value: false);
		extremeYangText.gameObject.SetActive(value: false);
		balanceText.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		TrigramsManager.YinYangStateType yinYangStateType = (UIBase.gameState.battleState.isInBattle ? UIBase.gameState.trigramsManager.GetCurrentState() : TrigramsManager.YinYangStateType.None);
		if (yinYangStateType != currentState)
		{
			currentState = yinYangStateType;
			UpdateStateTexts();
		}
	}

	private void UpdateStateTexts()
	{
		bool num = currentState.HasFlag(TrigramsManager.YinYangStateType.Yin);
		bool flag = currentState.HasFlag(TrigramsManager.YinYangStateType.ExtremeYin);
		bool shouldShow = num | flag;
		TextDisplayInfo textDisplayInfo = textDisplayInfos[0];
		textDisplayInfo.text.text = (flag ? ScriptableObjectSingleton<TextDataPreset>.instance.term_trigrams_extreme_yin : ScriptableObjectSingleton<TextDataPreset>.instance.term_trigrams_yin);
		UpdateStateText(shouldShow, textDisplayInfo);
		bool num2 = currentState.HasFlag(TrigramsManager.YinYangStateType.Yang);
		bool flag2 = currentState.HasFlag(TrigramsManager.YinYangStateType.ExtremeYang);
		shouldShow = num2 | flag2;
		textDisplayInfo = textDisplayInfos[1];
		textDisplayInfo.text.text = (flag2 ? ScriptableObjectSingleton<TextDataPreset>.instance.term_trigrams_extreme_yang : ScriptableObjectSingleton<TextDataPreset>.instance.term_trigrams_yang);
		UpdateStateText(shouldShow, textDisplayInfo);
	}

	private void UpdateStateText(bool shouldShow, TextDisplayInfo info)
	{
		if (shouldShow != info.showing)
		{
			info.showing = shouldShow;
			if (shouldShow)
			{
				ShowText(info);
			}
			else
			{
				HideText(info);
			}
		}
	}

	private async void ShowText(TextDisplayInfo info)
	{
		info.sequence?.Kill();
		info.sequence = DOTween.Sequence();
		info.sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		info.text.gameObject.SetActive(value: true);
		info.sequence.Append(info.text.DOFade(1f, 0.1f));
		Sequence sequence = info.sequence;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			info.sequence = null;
		});
		info.sequence.Play();
		await DOTweenModuleUnityVersion.AsyncWaitForKill((Tween)info.sequence);
	}

	private async void HideText(TextDisplayInfo info)
	{
		info.sequence?.Kill();
		info.sequence = DOTween.Sequence();
		info.sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		info.sequence.Append(info.text.DOFade(0f, 0.1f));
		Sequence sequence = info.sequence;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			info.sequence = null;
			info.text.gameObject.SetActive(value: false);
		});
		info.sequence.Play();
		await DOTweenModuleUnityVersion.AsyncWaitForKill((Tween)info.sequence);
	}
}
