using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI;

public class UIManager : UINode
{
	public List<IFrameUpdate> frameUpdaters = new List<IFrameUpdate>();

	public InfoPanelManager infoPanelManager = new InfoPanelManager();

	public List<IInputUpdate> inputUpdaters = new List<IInputUpdate>();

	public List<ILateUpdate> lateUpdaters = new List<ILateUpdate>();

	public void Init()
	{
		manager = this;
		OnInit();
	}

	protected virtual void OnInit()
	{
	}

	public void Dispose()
	{
		foreach (UIBase item in new List<UIBase>(children))
		{
			item._InternalDispose();
		}
		base.gameObject.SetActive(value: false);
		inputUpdaters.Clear();
		frameUpdaters.Clear();
		lateUpdaters.Clear();
		OnDispose();
	}

	protected virtual void OnDispose()
	{
	}

	public void FrameUpdate()
	{
		for (int i = 0; i < inputUpdaters.Count; i++)
		{
			if (!inputUpdaters[i].Equals(null) && IsUpdaterActive(inputUpdaters[i]))
			{
				TryInputUpdate(inputUpdaters[i]);
			}
		}
		for (int j = 0; j < frameUpdaters.Count; j++)
		{
			if (!frameUpdaters[j].Equals(null) && IsUpdaterActive(frameUpdaters[j]))
			{
				TryFrameUpdate(frameUpdaters[j]);
			}
		}
		if (UINode.hasDirty)
		{
			UINode.hasDirty = false;
			foreach (UIBase child in children)
			{
				child._InternalUpdate();
			}
		}
		for (int k = 0; k < lateUpdaters.Count; k++)
		{
			if (!lateUpdaters[k].Equals(null) && IsUpdaterActive(lateUpdaters[k]))
			{
				TryLateUpdate(lateUpdaters[k]);
			}
		}
	}

	private static bool IsUpdaterActive(object updater)
	{
		if (updater is Behaviour behaviour)
		{
			return behaviour.isActiveAndEnabled;
		}
		return true;
	}

	public override void SetDirty()
	{
		foreach (UIBase child in children)
		{
			child.SetDirty();
		}
		UINode.hasDirty = true;
	}

	public virtual void ShowUI(UIBase ui)
	{
		if (ui is IShowAsync showAsync)
		{
			StartCoroutine(showAsync.ShowUIAsync());
		}
		else
		{
			ui.SetActiveAndDirty(active: true);
		}
	}

	public virtual void HideUI(UIBase ui)
	{
		if (ui is IHideAsync hideAsync)
		{
			M_StartCoroutine(hideAsync.HideUIAsync());
		}
		else
		{
			ui.gameObject.SetActive(value: false);
		}
	}

	protected virtual void M_StartCoroutine(IEnumerator coroutine)
	{
		StartCoroutine(coroutine);
	}

	private void TryInputUpdate(IInputUpdate updater)
	{
		try
		{
			updater?.UpdateInput();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[UpdateInput] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryFrameUpdate(IFrameUpdate updater)
	{
		try
		{
			updater?.FrameUpdate();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[FrameUpdate] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryLateUpdate(ILateUpdate updater)
	{
		try
		{
			updater?.OnUpdate();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[lateUpdaters_OnUpdate] [{base.gameObject.name}] {arg}", this);
		}
	}
}
