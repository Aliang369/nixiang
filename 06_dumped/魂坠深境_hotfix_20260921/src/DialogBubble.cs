using System;
using System.Collections;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogBubble : CellUI<DialoguePlayer>, IFrameUpdate
{
	public enum StopType
	{
		BreakStop,
		Finish
	}

	public TextMeshProUGUI text;

	public DialoguePlayer player;

	public bool skipFlag;

	public Vector2 safeRegionX;

	private Action<StopType> currentOnStopAction;

	private WrapperEnumerator currentSequenceDisplayCoroutine;

	private DialoguePanel attachDialoguePanel;

	public bool ShouldActive
	{
		get
		{
			if (player != null)
			{
				return player.isActiveAndEnabled;
			}
			return false;
		}
	}

	private float GameSpeedMultiplay => 1f;

	protected override void Init()
	{
		attachDialoguePanel = GetUIInParents<DialoguePanel>();
		float width = (base.transform.parent as RectTransform).GetWidth();
		safeRegionX.x = (0f - width) / 2f + base.rectTransform.GetWidth();
		safeRegionX.y = width / 2f;
	}

	protected override void OnSetup(DialoguePlayer dialoguePlayer)
	{
		GetComponent<CanvasGroup>().alpha = 0f;
		player = dialoguePlayer;
		UpdatePosition();
		ShowBoxArea();
		TextmeshproEffect componentInChildren = GetComponentInChildren<TextmeshproEffect>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.rollStrength = new Vector2(0f, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueRollStrength);
			componentInChildren.rollSpeed = ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueRollSpeed;
			componentInChildren.jitterStrength = new Vector2(ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueJitterStrength, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueJitterStrength);
		}
	}

	public void Stop(StopType type)
	{
		skipFlag = false;
		if (currentSequenceDisplayCoroutine != null)
		{
			currentSequenceDisplayCoroutine.Stop();
			currentSequenceDisplayCoroutine = null;
		}
		if (currentOnStopAction != null)
		{
			currentOnStopAction(type);
			currentOnStopAction = null;
		}
		attachDialoguePanel.dialogueBubblePool.Despawn(this);
	}

	void IFrameUpdate.FrameUpdate()
	{
		UpdatePosition();
	}

	public void PlayDialogueSequence(DialogueSequence sequence, Action<StopType> onStop)
	{
		currentOnStopAction = onStop;
		currentSequenceDisplayCoroutine = CellUI<DialoguePlayer>.applicationState.StartCoroutine(() => DisplayDialogueSequenceCor(sequence));
	}

	private IEnumerator DisplayDialogueSequenceCor(DialogueSequence sequence)
	{
		foreach (DialogueText dialogText in sequence.dialogueSequence)
		{
			if (dialogText.prewarmTime != 0f)
			{
				yield return HideBoxArea();
				yield return new WaitForSeconds(dialogText.prewarmTime / GameSpeedMultiplay);
			}
			yield return ShowBoxArea();
			yield return text.ScrollTextCor(dialogText.text, dialogText.ReadSpeed * GameSpeedMultiplay, IsToSkipDialogue);
			skipFlag = false;
			float timer = 0f;
			while (true)
			{
				timer += Time.deltaTime * GameSpeedMultiplay;
				if (timer > dialogText.AfterWaitTime)
				{
					break;
				}
				if (IsToSkipDialogue())
				{
					skipFlag = false;
					break;
				}
				yield return new WaitForEndOfFrame();
			}
			skipFlag = false;
		}
		yield return HideBoxArea();
		Stop(StopType.Finish);
		bool IsToSkipDialogue()
		{
			if (skipFlag)
			{
				return !sequence.disableSkip;
			}
			return false;
		}
	}

	public void SetText(string str)
	{
		text.text = str;
	}

	public void SkipNextDialogue()
	{
		skipFlag = true;
	}

	private void UpdatePosition()
	{
		if (!(player == null))
		{
			base.rectTransform.position = ((!player.IsWorldObjectPlayer) ? player.transform.position : (CellUI<DialoguePlayer>.gameState?.UIManager.gameVirtualCamera.bindedCamera.finalCamera.WorldToScreenPoint(player.transform.position) ?? Vector3.zero));
			UnityHelper.TargetRectInAreaRect(base.rectTransform, base.rectTransform.parent as RectTransform);
		}
	}

	private IEnumerator HideBoxArea()
	{
		if (GetComponent<CanvasGroup>().alpha != 0f)
		{
			GetComponent<CanvasGroup>().alpha = 1f;
			DOTweenModuleUI.DOFade(GetComponent<CanvasGroup>(), 0f, 0.5f);
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator ShowBoxArea()
	{
		if (GetComponent<CanvasGroup>().alpha != 1f)
		{
			GetComponent<CanvasGroup>().alpha = 0f;
			DOTweenModuleUI.DOFade(GetComponent<CanvasGroup>(), 1f, 0.5f);
		}
		yield break;
	}
}
