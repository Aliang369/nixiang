using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideHintPanelSet : UIBase
{
	[SerializeField]
	public List<GuideHintPanel> guideHintPanels;

	[NonSerialized]
	private int index;

	[NonSerialized]
	private Action confirmCallback;

	private const float loadVideoDeadline = 1.5f;

	protected override void Init()
	{
		for (int i = 0; i < guideHintPanels.Count; i++)
		{
			InitUI(guideHintPanels[i]);
			guideHintPanels[i].Setup(i);
		}
	}

	public void Show(Action afterConfirmCallback = null)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		confirmCallback = afterConfirmCallback;
		base.gameObject.SetActive(value: true);
		index = -1;
		ShowNext();
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_hint_pop_up);
	}

	public void TryFirstOpenAutoPopup()
	{
		if (!UIBase.applicationState.guideManager.autoPopupedHints.Contains(base.gameObject.name) && !UIBase.applicationState.guideManager.skipGuide)
		{
			ShowAndMarkAutoPopuped();
		}
	}

	public void ShowAndMarkAutoPopuped(Action afterConfirmCallback = null)
	{
		Show(afterConfirmCallback);
		MarkAutoPopuped();
	}

	public void MarkAutoPopuped()
	{
		if (!UIBase.applicationState.guideManager.autoPopupedHints.Contains(base.gameObject.name))
		{
			UIBase.applicationState.guideManager.autoPopupedHints.Add(base.gameObject.name);
		}
	}

	public void ShowNext()
	{
		index++;
		StartCoroutine(ShowGuidePanelCor(guideHintPanels[index]));
	}

	public void ShowPrevious()
	{
		index--;
		StartCoroutine(ShowGuidePanelCor(guideHintPanels[index]));
	}

	private IEnumerator ShowGuidePanelCor(GuideHintPanel guidePanel)
	{
		if ((UnityEngine.Object)(object)guidePanel.presetVideo != null)
		{
			guidePanel.PrepareVideo(guidePanel.presetVideo);
			long loadStartTime = DateTime.Now.Ticks;
			yield return new WaitUntil(delegate
			{
				bool flag = (float)(DateTime.Now.Ticks - loadStartTime) / 10000000f > 1.5f;
				return guidePanel.IsVideoPlayerPrepared() | flag;
			});
		}
		guidePanel.ShowHint();
	}

	public void Confirm()
	{
		confirmCallback?.Invoke();
		confirmCallback = null;
		base.gameObject.SetActive(value: false);
	}
}
