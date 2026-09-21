using Common.UI;

public static class FortressUI
{
	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public static FortressStateInfo fortressState => applicationState.fortressState;

	public static FortressUIManager fortressUIManager => fortressState.uiManager;

	public static void ShowUI(this Common.UI.UIBase ui)
	{
		if (ui is IStackPanel { stack: not false })
		{
			ui.transform.parent = fortressState.uiManager.stackedPanelPivot;
			ui.transform.SetAsLastSibling();
		}
		fortressUIManager.ShowUI(ui);
	}

	public static void ShowUI<T>(this CellUI<T> cellUI, T setupData)
	{
		cellUI.Setup(setupData);
		if (cellUI is IStackPanel { stack: not false })
		{
			cellUI.transform.parent = fortressState.uiManager.stackedPanelPivot;
			cellUI.transform.SetAsLastSibling();
		}
		fortressUIManager.ShowUI(cellUI);
	}

	public static void HideUI(this UIBase ui)
	{
		fortressUIManager.HideUI(ui);
	}

	public static void HideUI<T>(this CellUI<T> cellUI)
	{
		cellUI.Unset();
		fortressUIManager.HideUI(cellUI);
	}
}
