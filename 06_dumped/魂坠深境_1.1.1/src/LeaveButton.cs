using System;
using DG.Tweening;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaveButton : UIBase
{
	private static class EventName
	{
		public const string Text_in = "Text_in";

		public const string Text_out = "Text_out";
	}

	[SerializeField]
	public GameButton button;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private SkeletonGraphic skeletonGraphic;

	[SerializeField]
	private Image background;

	[NonSerialized]
	private Animation anim_Idle;

	[NonSerialized]
	private Animation anim_In;

	[NonSerialized]
	private Animation anim_Out;

	[NonSerialized]
	public bool isEnable;

	[NonSerialized]
	public bool isPlaying;

	[NonSerialized]
	private bool hideNext;

	[NonSerialized]
	private bool isTextShow;

	[NonSerialized]
	public Action onLeave;

	[NonSerialized]
	private DG.Tweening.Sequence sequence;

	private Animation currentAnim => skeletonGraphic.GetCurrentAnimation();

	protected override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
		skeletonGraphic.AnimationState.Event += _OnEvent;
		skeletonGraphic.AnimationState.Complete += _OnComplete;
		ExposedList<Animation> animations = skeletonGraphic.SkeletonData.Animations;
		anim_In = animations.Find((Animation a) => a.Name == "in");
		anim_Idle = animations.Find((Animation a) => a.Name == "idle");
		anim_Out = animations.Find((Animation a) => a.Name == "out");
		base.gameObject.SetActive(value: false);
		hideNext = false;
		if (background != null)
		{
			background.gameObject.SetActive(value: false);
		}
	}

	public void Show()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (isEnable)
		{
			return;
		}
		if (hideNext)
		{
			hideNext = false;
			return;
		}
		base.gameObject.SetActive(value: true);
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_scroll);
		PlayAnim(anim_In);
		isTextShow = false;
		text.alpha = 0f;
		isEnable = true;
		if (background != null)
		{
			background.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (isEnable)
		{
			base.gameObject.SetActive(value: false);
			isEnable = false;
			if (background != null)
			{
				background.gameObject.SetActive(value: false);
			}
		}
	}

	public void HideNextLeaveButton()
	{
		hideNext = true;
	}

	public bool HasAnim()
	{
		return isPlaying;
	}

	private void ShowText()
	{
		if (!isTextShow)
		{
			isTextShow = true;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = base.uiTimeScale;
			sequence.Append(text.DOFade(1f, 0.5f));
			DG.Tweening.Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	private void HideText()
	{
		if (isTextShow)
		{
			isTextShow = false;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = base.uiTimeScale;
			sequence.Append(text.DOFade(0f, 0.2f));
			DG.Tweening.Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	private void PlayAnim(Animation anim, int? timeScale = null, bool loop = false)
	{
		isPlaying = true;
		skeletonGraphic.timeScale = ((float?)timeScale) ?? base.uiTimeScale;
		skeletonGraphic.PlayAnimation(anim, loop);
	}

	private void _OnButtonClicked()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (isEnable && (UIBase.gameState == null || UIBase.gameState.actionManager.isEmpty))
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_scroll_click);
			isEnable = false;
			skeletonGraphic.timeScale = base.uiTimeScale;
			PlayAnim(anim_Out);
			if (background != null)
			{
				background.gameObject.SetActive(value: false);
			}
		}
	}

	private void _OnEvent(TrackEntry entry, Event e)
	{
		string text = e.Data.Name;
		if (!(text == "Text_in"))
		{
			if (text == "Text_out")
			{
				HideText();
			}
		}
		else
		{
			ShowText();
		}
	}

	private void _OnComplete(TrackEntry entry)
	{
		isPlaying = false;
		if (entry.Animation == anim_In)
		{
			PlayAnim(anim_Idle, 1, loop: true);
		}
		else if (entry.Animation == anim_Out)
		{
			onLeave?.Invoke();
		}
		skeletonGraphic.timeScale = 1f;
	}
}
