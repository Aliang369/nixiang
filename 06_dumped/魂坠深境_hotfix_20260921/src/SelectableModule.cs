using System;
using UnityEngine;

[RequireComponent(typeof(InputModule))]
public class SelectableModule : SelectableModuleBase
{
	[SerializeField]
	private bool usePointerClick;

	public override void Init()
	{
		base.Init();
		InputModule module = parent.GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
		if (usePointerClick)
		{
			module.SetEnable(InputModuleBase.EventType.PointerClick, enable: true);
		}
		else
		{
			module.SetEnable(InputModuleBase.EventType.PointerDown, enable: true);
		}
	}

	private void _OnHandleInput(InputModuleBase inputModule)
	{
		if (usePointerClick)
		{
			_OnHandleInput_PointerClick(inputModule);
		}
		else
		{
			_OnHandleInput_PointerDown(inputModule);
		}
	}

	private void _OnHandleInput_PointerClick(InputModuleBase inputModule)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!inputModule.pointerClicked || inputModule.lastPointerDownTime < inputModule.lastDragTime)
		{
			return;
		}
		selectableArea.OnClicked(this);
		if (!selectableArea.IsSelected(this))
		{
			if (!selectableArea.CanSelect(this))
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_nagative);
			}
			else
			{
				selectableArea.Select(this);
			}
		}
		else
		{
			selectableArea.Unselect(this);
		}
	}

	private void _OnHandleInput_PointerDown(InputModuleBase inputModule)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!inputModule.pointerDown)
		{
			return;
		}
		selectableArea.OnClicked(this);
		if (!selectableArea.IsSelected(this))
		{
			if (!selectableArea.CanSelect(this))
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_nagative);
			}
			else
			{
				selectableArea.Select(this);
			}
		}
		else
		{
			selectableArea.Unselect(this);
		}
	}
}
