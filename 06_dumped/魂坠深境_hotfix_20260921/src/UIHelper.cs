using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public static class UIHelper
{
	private static bool hidingAllInfo;

	private static ApplicationStateInfo applicationState => Bootstrap.instance.CurrentApplicationState;

	public static void Init()
	{
	}

	public static void EnableAllButton()
	{
		GameButton.enableAll = true;
	}

	public static void DisableAllButton()
	{
		GameButton.enableAll = false;
	}

	public static void HideAllInfo(Transform hideSource)
	{
		if (hidingAllInfo)
		{
			Debug.LogError("正在隐藏所有信息面板，不能重复调用");
			return;
		}
		hidingAllInfo = true;
		try
		{
			Bootstrap.instance.CurrentApplicationState?.eventManager?.UIHideAllInfoEvent.TriggerEvent(hideSource);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
		finally
		{
			hidingAllInfo = false;
		}
	}

	public static void RegisterUIHideAllInfoEvent(this Common.UI.UIManager uiManager, IInfoPanelController controller)
	{
		uiManager.infoPanelManager.AddController(controller);
	}

	public static void UnregisterUIHideAllInfoEvent(this Common.UI.UIManager uiManager, IInfoPanelController controller)
	{
		uiManager.infoPanelManager.RemoveController(controller);
	}

	public static void RegisterEmptyAreaClickedEvent(Action action)
	{
		applicationState.eventManager.UIEmptyClickedEvent.AddListener(action);
	}

	public static void UnregisterEmptyAreaClickedEvent(Action action)
	{
		applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(action);
	}

	public static UniversalCommonInfoPanel GetDefaultCommonInfoPanel()
	{
		if (Bootstrap.applicationState.fortressState != null)
		{
			return Bootstrap.applicationState.fortressState.uiManager.universalCommonInfoPanel;
		}
		if (Bootstrap.applicationState.runningGameState != null)
		{
			return Bootstrap.applicationState.runningGameState.UIManager.universalCommonInfoPanel;
		}
		return null;
	}

	public static void RefreshLayout(this RectTransform rectTransform)
	{
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		Canvas.ForceUpdateCanvases();
	}
}
