using System;
using Common.UI;
using UnityEngine;

[RequireComponent(typeof(InputModuleBase))]
public class InfoModule_InputModule : InfoModule
{
	[SerializeField]
	private bool useEventSystem = true;

	[SerializeField]
	private RectTransform passRect;

	[NonSerialized]
	private InputModuleBase inputModule;

	[NonSerialized]
	private bool block;

	[NonSerialized]
	private bool previousOver;

	public override void Init()
	{
		inputModule = parent.GetModule<InputModuleBase>();
		InputModuleBase inputModuleBase = inputModule;
		inputModuleBase.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(inputModuleBase.onHandleInput, new Action<InputModuleBase>(OnHandleInput));
	}

	private void OnHandleInput(InputModuleBase _)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (inputModule.pointerClicked && (!inputModule.enables.HasFlag(InputModuleBase.EventType.PointerDown) || inputModule.lastPointerDownTime > inputModule.lastDragTime))
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			base.infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Click, passRect, false));
		}
		if (useEventSystem)
		{
			HandleEventSystemInput();
		}
		else
		{
			HandleRawInput();
		}
	}

	private void HandleEventSystemInput()
	{
		if (!PlatformHelper.IsMobile())
		{
			if (inputModule.pointerEnter)
			{
				base.infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Hover, passRect, false));
			}
			if (inputModule.pointerExit)
			{
				base.infoPanel?.HideCommonInfo(parent._setupData, CommonInfoData.ControlType.Hover);
			}
		}
	}

	private void HandleRawInput()
	{
		if (PlatformHelper.IsMobile())
		{
			return;
		}
		bool flag = inputModule.over || (inputModule.rawOver && inputModule.CheckIsBlockedByInfo());
		if (previousOver != flag)
		{
			previousOver = flag;
			if (flag)
			{
				base.infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Hover, passRect, false));
			}
			else
			{
				base.infoPanel?.HideCommonInfo(parent._setupData, CommonInfoData.ControlType.Hover);
			}
		}
	}
}
