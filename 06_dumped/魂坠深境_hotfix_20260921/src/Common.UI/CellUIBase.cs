using System;
using UnityEngine;

namespace Common.UI;

public abstract class CellUIBase<TSetupData> : UIBase
{
	protected override bool shouldUpdate
	{
		get
		{
			if (!base.shouldUpdate)
			{
				return false;
			}
			if (_setupData == null)
			{
				Debug.LogWarning("!_setupData == null " + base.gameObject.name, this);
				return false;
			}
			return true;
		}
	}

	public TSetupData setupData => (TSetupData)_setupData;

	public void Setup(TSetupData setupData, params (object key, object value)[] appendData)
	{
		if (!inited)
		{
			Debug.LogError("!inited " + base.gameObject.name, this);
			return;
		}
		if (setupData == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		_setupData = setupData;
		_appendData = appendData;
		TrySetup();
		if (this is IDisplayModulesController)
		{
			SetupDisplayModules();
		}
		OnLateSetup();
		SetDirty();
	}

	public void Unset()
	{
		if (!inited)
		{
			Debug.LogError("!inited " + base.gameObject.name, this);
			return;
		}
		if (this is IDisplayModulesController)
		{
			UnsetDisplayModules();
		}
		TryUnset();
		_setupData = null;
		_appendData = null;
	}

	protected virtual void OnSetup(TSetupData setupData)
	{
	}

	protected virtual void OnLateSetup()
	{
	}

	protected virtual void OnUnset()
	{
	}

	private void TrySetup()
	{
		try
		{
			OnSetup(setupData);
		}
		catch (Exception arg)
		{
			Debug.LogError($"[OnSetup] [{base.gameObject.name}] {arg}", this);
		}
	}

	private void TryUnset()
	{
		try
		{
			OnUnset();
		}
		catch (Exception arg)
		{
			Debug.LogError($"[OnUnset] [{base.gameObject.name}] {arg}", this);
		}
	}
}
