using System;
using UnityEngine.UIElements;

public class zbUIEvent
{
	private Action m_UnregisterActions;

	public void RegisterCallback<TEvent>(VisualElement visualElement, Action<TEvent> callback, string strAudioPath = "", TrickleDown useTrickleDown = (TrickleDown)0) where TEvent : EventBase<TEvent>, new()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (typeof(ClickEvent).IsAssignableFrom(typeof(TEvent)))
		{
			VisualElement obj = visualElement;
			Button btn = (Button)(object)((obj is Button) ? obj : null);
			if (btn != null)
			{
				EventCallback<ClickEvent> soundCallback = delegate
				{
					OnBtnClickedSound(strAudioPath);
				};
				((CallbackEventHandler)btn).RegisterCallback<ClickEvent>(soundCallback, (TrickleDown)0);
				m_UnregisterActions = (Action)Delegate.Combine(m_UnregisterActions, (Action)delegate
				{
					((CallbackEventHandler)btn).UnregisterCallback<ClickEvent>(soundCallback, (TrickleDown)0);
				});
			}
		}
		EventCallback<TEvent> eventCallback = delegate(TEvent evt)
		{
			ClickEvent val = (ClickEvent)((((object)evt) is ClickEvent) ? ((object)evt) : null);
			if (val != null)
			{
				ClickEvent cloned = CloneClickEvent(val);
				visualElement.schedule.Execute((Action)delegate
				{
					try
					{
						Action<TEvent> action = callback;
						ClickEvent obj2 = cloned;
						action((TEvent)(object)((obj2 is TEvent) ? obj2 : null));
					}
					finally
					{
						((EventBase)cloned).Dispose();
					}
				}).StartingIn(0L);
			}
			else
			{
				callback(evt);
			}
		};
		((CallbackEventHandler)visualElement).RegisterCallback<TEvent>(eventCallback, useTrickleDown);
		m_UnregisterActions = (Action)Delegate.Combine(m_UnregisterActions, (Action)delegate
		{
			((CallbackEventHandler)visualElement).UnregisterCallback<TEvent>(eventCallback, (TrickleDown)0);
		});
	}

	public void RegisterCallback<TEvent>(VisualElement visualElement, Action callback, string strAudioPath = "", TrickleDown useTrickleDown = (TrickleDown)0) where TEvent : EventBase<TEvent>, new()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (typeof(ClickEvent).IsAssignableFrom(typeof(TEvent)))
		{
			VisualElement obj = visualElement;
			Button btn = (Button)(object)((obj is Button) ? obj : null);
			if (btn != null)
			{
				EventCallback<ClickEvent> soundCallback = delegate
				{
					OnBtnClickedSound(strAudioPath);
				};
				((CallbackEventHandler)btn).RegisterCallback<ClickEvent>(soundCallback, (TrickleDown)0);
				m_UnregisterActions = (Action)Delegate.Combine(m_UnregisterActions, (Action)delegate
				{
					((CallbackEventHandler)btn).UnregisterCallback<ClickEvent>(soundCallback, (TrickleDown)0);
				});
			}
		}
		EventCallback<TEvent> eventCallback = delegate(TEvent evt)
		{
			if (evt is ClickEvent)
			{
				visualElement.schedule.Execute((Action)delegate
				{
					callback();
				}).StartingIn(0L);
			}
			else
			{
				callback();
			}
		};
		((CallbackEventHandler)visualElement).RegisterCallback<TEvent>(eventCallback, useTrickleDown);
		m_UnregisterActions = (Action)Delegate.Combine(m_UnregisterActions, (Action)delegate
		{
			((CallbackEventHandler)visualElement).UnregisterCallback<TEvent>(eventCallback, (TrickleDown)0);
		});
	}

	public void RegisterValueChangedCallback<T>(BindableElement bindableElement, Action<T> callback) where T : struct
	{
		EventCallback<ChangeEvent<T>> eventCallback = delegate(ChangeEvent<T> evt)
		{
			callback(evt.newValue);
		};
		((CallbackEventHandler)bindableElement).RegisterCallback<ChangeEvent<T>>(eventCallback, (TrickleDown)0);
		m_UnregisterActions = (Action)Delegate.Combine(m_UnregisterActions, (Action)delegate
		{
			((CallbackEventHandler)bindableElement).UnregisterCallback<ChangeEvent<T>>(eventCallback, (TrickleDown)0);
		});
	}

	public void RegisterValueChangedCallback(BindableElement bindableElement, Action<string> callback)
	{
		EventCallback<ChangeEvent<string>> eventCallback = delegate(ChangeEvent<string> evt)
		{
			callback(evt.newValue);
		};
		((CallbackEventHandler)bindableElement).RegisterCallback<ChangeEvent<string>>(eventCallback, (TrickleDown)0);
		m_UnregisterActions = (Action)Delegate.Combine(m_UnregisterActions, (Action)delegate
		{
			((CallbackEventHandler)bindableElement).UnregisterCallback<ChangeEvent<string>>(eventCallback, (TrickleDown)0);
		});
	}

	public void UnRegisterCallback()
	{
		m_UnregisterActions?.Invoke();
		m_UnregisterActions = null;
	}

	private void OnBtnClickedSound(string strAudioPath)
	{
		if (string.IsNullOrEmpty(strAudioPath))
		{
			strAudioPath = zbAudioDefine.UI_CLICK;
		}
		zbAudioMgr.Instance.PlayAudio(strAudioPath);
	}

	private ClickEvent CloneClickEvent(ClickEvent src)
	{
		ClickEvent pooled = PointerEventBase<ClickEvent>.GetPooled((IPointerEvent)(object)src);
		((EventBase)pooled).target = ((EventBase)src).target;
		return pooled;
	}
}
