using System;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameButton : Button
{
	public static bool enableAll = true;

	[SerializeField]
	public bool hideAllInfo = true;

	[SerializeField]
	public bool changeChildrenAlphaWhenDisable = true;

	[SerializeField]
	public float customAlpha = -1f;

	[SerializeField]
	public EventReference clickSoundRef;

	[SerializeField]
	public EventReference negativeClickSoundRef;

	public Action onHighlight;

	public Action onUnhighlight;

	public ButtonClickedEvent onNotInteractableClick = new ButtonClickedEvent();

	[SerializeField]
	public bool disableTimeCheck;

	[NonSerialized]
	protected SelectionState previousState;

	[NonSerialized]
	private DateTime enableTime = DateTime.MinValue;

	public new bool interactable
	{
		get
		{
			return base.interactable;
		}
		set
		{
			base.interactable = value;
			if (changeChildrenAlphaWhenDisable)
			{
				TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
				foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
				{
					textMeshProUGUI.color = (value ? textMeshProUGUI.color.SetA(1f) : textMeshProUGUI.color.SetA((customAlpha < 0f) ? 0.5f : customAlpha));
				}
			}
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (!enableAll)
		{
			return;
		}
		if (!disableTimeCheck)
		{
			if (DateTime.UtcNow < enableTime)
			{
				return;
			}
			enableTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.20000000298023224);
		}
		if (hideAllInfo)
		{
			UIHelper.HideAllInfo(base.transform);
		}
		if (!interactable)
		{
			onNotInteractableClick?.Invoke();
		}
		PlaySound(interactable ? clickSoundRef : negativeClickSoundRef);
		base.OnPointerClick(eventData);
	}

	private void PlaySound(EventReference soundRef)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!((EventReference)(ref soundRef)).IsNull)
		{
			if (Bootstrap.instance?.audioManager != null)
			{
				Bootstrap.instance.audioManager.PlayOneShot(soundRef);
			}
			else
			{
				RuntimeManager.PlayOneShot(soundRef, default(Vector3));
			}
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		SelectionState selectionState = previousState;
		previousState = state;
		base.DoStateTransition(state, instant);
		if (state == SelectionState.Highlighted)
		{
			onHighlight?.Invoke();
		}
		else if (selectionState == SelectionState.Highlighted && state == SelectionState.Normal)
		{
			onUnhighlight?.Invoke();
		}
	}
}
