using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.Accounts.Utils.UI;
using DG.Tweening;
using UnityEngine;

namespace Common.UI;

public abstract class UIBase : UINode
{
	public static class Events
	{
		public static Action<UINode> onSetDirty;
	}

	[SerializeField]
	private UIBase[] subUI;

	[NonSerialized]
	private bool dirty;

	[NonSerialized]
	protected UIDisplayModuleController displayModulesController;

	private Dictionary<object, Action> events;

	[NonSerialized]
	private CanvasGroup canvasGroupComponentCache;

	protected virtual bool shouldUpdate
	{
		get
		{
			if (this != null)
			{
				return base.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	private void InitDisplayModules()
	{
		if (displayModulesController == null)
		{
			return;
		}
		List<MonoDisplayModule> displayModules = displayModulesController.displayModules;
		for (int i = 0; i < displayModules.Count; i++)
		{
			displayModules[i].parent = this;
		}
		for (int j = 0; j < displayModules.Count; j++)
		{
			displayModules[j].OnConfigContext();
			TryInit(displayModules[j]);
			if (displayModules[j] is IInputUpdate item)
			{
				manager.inputUpdaters.Add(item);
			}
			if (displayModules[j] is IFrameUpdate item2)
			{
				manager.frameUpdaters.Add(item2);
			}
			if (displayModules[j] is ILateUpdate item3)
			{
				manager.lateUpdaters.Add(item3);
			}
		}
	}

	protected void SetupDisplayModules()
	{
		if (displayModulesController == null)
		{
			return;
		}
		foreach (MonoDisplayModule displayModule in displayModulesController.displayModules)
		{
			TrySetup(displayModule);
		}
	}

	private void UpdateDisplayModules()
	{
		if (displayModulesController == null)
		{
			return;
		}
		foreach (MonoDisplayModule displayModule in displayModulesController.displayModules)
		{
			TryUpdateDisplay(displayModule);
		}
	}

	protected void UnsetDisplayModules()
	{
		if (displayModulesController == null)
		{
			return;
		}
		foreach (MonoDisplayModule displayModule in displayModulesController.displayModules)
		{
			TryUnset(displayModule);
		}
	}

	private void DisposeDisplayModules()
	{
		if (displayModulesController == null)
		{
			return;
		}
		List<MonoDisplayModule> displayModules = displayModulesController.displayModules;
		for (int i = 0; i < displayModules.Count; i++)
		{
			TryDispose(displayModules[i]);
			if (displayModules[i] is IInputUpdate item)
			{
				manager.inputUpdaters.Remove(item);
			}
			if (displayModules[i] is IFrameUpdate item2)
			{
				manager.frameUpdaters.Remove(item2);
			}
			if (displayModules[i] is ILateUpdate item3)
			{
				manager.lateUpdaters.Remove(item3);
			}
		}
		displayModules.Clear();
	}

	private void TryInit(MonoDisplayModule module)
	{
		try
		{
			module?.Init();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[displayModules[].Init()] - [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TrySetup(MonoDisplayModule module)
	{
		try
		{
			module?.Setup();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[displayModules[].Setup()] - [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryUpdateDisplay(MonoDisplayModule module)
	{
		try
		{
			module?.UpdateDisplay();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[displayModules[].UpdateDisplay()] - [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryUnset(MonoDisplayModule module)
	{
		try
		{
			module?.Unset();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[displayModules[].Unset()] - [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryDispose(MonoDisplayModule module)
	{
		try
		{
			module?.Dispose();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[displayModules[].Dispose()] - [{base.gameObject.name}] {arg}", this);
		}
	}

	public override void SetDirty()
	{
		dirty = true;
		UINode.hasDirty = true;
		Events.onSetDirty?.Invoke(this);
	}

	public void ForceUpdate()
	{
		_InternalUpdate(hierarchy: false, forceUpdate: true);
	}

	protected virtual void Init()
	{
	}

	protected virtual void Dispose()
	{
	}

	protected virtual void UpdateDisplay()
	{
	}

	internal void _InternalInit()
	{
		updating = false;
		IDisplayModulesController displayModulesController = this as IDisplayModulesController;
		if (displayModulesController != null)
		{
			this.displayModulesController = new UIDisplayModuleController(this);
		}
		if (subUI != null)
		{
			UIBase[] array = subUI;
			foreach (UIBase uIBase in array)
			{
				if (uIBase == null)
				{
					Debug.LogError(base.gameObject.name + "的subUI有空物体");
				}
				else
				{
					TryInit(uIBase);
				}
			}
		}
		TryInit();
		if (displayModulesController != null)
		{
			InitDisplayModules();
		}
		inited = true;
	}

	internal void _InternalDispose()
	{
		foreach (PrefabPoolBase item in new List<PrefabPoolBase>(pools))
		{
			TryDispose(item);
		}
		foreach (UIBase child in children)
		{
			if (child is IUnset cellUI)
			{
				TryUnset(cellUI);
			}
			TryInternalDispose(child);
		}
		children.Clear();
		if (this is IDisplayModulesController)
		{
			DisposeDisplayModules();
		}
		TryDispose();
	}

	internal void _InternalUpdate(bool hierarchy = false, bool forceUpdate = false)
	{
		updating = true;
		bool flag = ((dirty | hierarchy) && shouldUpdate) | forceUpdate;
		if (flag)
		{
			dirty = false;
		}
		if (flag)
		{
			UpdateDisplay();
			if (this is IDisplayModulesController)
			{
				UpdateDisplayModules();
			}
		}
		foreach (UIBase child in children)
		{
			TryInternalUpdate(child, flag);
		}
		if (childrenTemp != null)
		{
			foreach (UIBase item in childrenTemp)
			{
				TryInternalUpdate(item, flag);
			}
		}
		updating = false;
		_InternalHandleChildren();
	}

	public T GetModule<T>() where T : MonoDisplayModule
	{
		UIDisplayModuleController uIDisplayModuleController = displayModulesController;
		if (uIDisplayModuleController == null)
		{
			return null;
		}
		return uIDisplayModuleController.GetModule<T>();
	}

	protected void AddListener(object key, Action action)
	{
		if (events == null)
		{
			events = new Dictionary<object, Action>();
		}
		if (!events.ContainsKey(key))
		{
			events[key] = action;
			return;
		}
		Dictionary<object, Action> dictionary = events;
		dictionary[key] = (Action)Delegate.Combine(dictionary[key], action);
	}

	protected void RemoveListener(object key, Action action)
	{
		Dictionary<object, Action> dictionary = events;
		dictionary[key] = (Action)Delegate.Remove(dictionary[key], action);
		if (events[key].GetInvocationList().Length == 0)
		{
			events.Remove(key);
		}
		if (events.Count == 0)
		{
			events = null;
		}
	}

	public void TriggerEvent(object key)
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].TriggerEvent(key);
		}
		if (events != null)
		{
			events.TryGetValue(key, out var value);
			value?.Invoke();
		}
	}

	public T GetUIInParents<T>()
	{
		return GetComponentsInParent<T>(includeInactive: true).FirstOrDefault();
	}

	public void SetActiveAndDirty(bool active)
	{
		if (((UnityEngine.Object)(object)canvasGroupComponentCache != null) & active)
		{
			canvasGroupComponentCache.alpha = 1f;
		}
		base.gameObject.SetActive(active);
		SetDirty();
	}

	public void SetActiveAndDirtyWithFadeAnim(bool active)
	{
		canvasGroupComponentCache = base.transform.GetOrAddComponent<CanvasGroup>();
		if (active)
		{
			base.gameObject.SetActive(value: true);
			SetDirty();
			canvasGroupComponentCache.alpha = 0f;
			DOTweenModuleUI.DOFade(canvasGroupComponentCache, 1f, 0.15f);
		}
		else
		{
			DOTweenModuleUI.DOFade(canvasGroupComponentCache, 0f, 0.15f).OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
				SetDirty();
			});
		}
	}

	public UINode GetUINode()
	{
		return this;
	}

	private void TryInit()
	{
		try
		{
			Init();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[Init] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryInit(UIBase ui)
	{
		try
		{
			InitUI(ui);
		}
		catch (Exception arg)
		{
			Debug.LogError($"[InitUI] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryUnset(IUnset cellUI)
	{
		try
		{
			cellUI.Unset();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[cellUI_Unset] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryDispose(PrefabPoolBase pool)
	{
		try
		{
			pool.Dispose();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[pool_Dispose] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryDispose()
	{
		try
		{
			Dispose();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[Dispose] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryInternalDispose(UIBase ui)
	{
		try
		{
			ui._InternalDispose();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[ui_InternalDispose] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryInternalUpdate(UIBase ui, bool update)
	{
		try
		{
			ui._InternalUpdate(update);
		}
		catch (Exception arg)
		{
			Debug.LogError($"[children_InternalUpdate] [{base.gameObject.name}] {arg}", this);
		}
	}
}
