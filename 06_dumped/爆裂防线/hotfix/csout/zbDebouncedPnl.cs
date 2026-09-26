using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDebouncedPnl : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbDebouncedPnl, UxmlTraits>
	{
	}

	public class UxmlTraits : UxmlTraits
	{
		private readonly UxmlFloatAttributeDescription m_interval;

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			((UxmlTraits)this).Init(ve, bag, cc);
			((zbDebouncedPnl)(object)ve).Interval = ((TypedUxmlAttributeDescription<float>)(object)m_interval).GetValueFromBag(bag, cc);
		}

		public UxmlTraits()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0021: Expected O, but got Unknown
			UxmlFloatAttributeDescription val = new UxmlFloatAttributeDescription
			{
				name = "interval"
			};
			((TypedUxmlAttributeDescription<float>)val).defaultValue = 1.5f;
			m_interval = val;
			((UxmlTraits)this)._002Ector();
		}
	}

	private double _lastClickTime;

	public float Interval { get; set; } = 1.5f;

	public event Action OnDebouncedClick;

	public zbDebouncedPnl()
	{
		((VisualElement)this).pickingMode = (PickingMode)0;
	}

	public void RegisterClickEvent(zbBaseWnd wnd, Action action)
	{
		wnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)this, (Action<ClickEvent>)OnClick, "", (TrickleDown)0);
		OnDebouncedClick -= action;
		OnDebouncedClick += action;
	}

	private void OnClick(ClickEvent evt)
	{
		((EventBase)evt).StopImmediatePropagation();
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		if (!(realtimeSinceStartupAsDouble - _lastClickTime < (double)Interval))
		{
			_lastClickTime = realtimeSinceStartupAsDouble;
			OnDebouncedClick?.Invoke();
		}
	}
}
