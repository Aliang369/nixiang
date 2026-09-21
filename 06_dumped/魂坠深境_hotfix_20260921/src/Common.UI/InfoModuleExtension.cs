namespace Common.UI;

public static class InfoModuleExtension
{
	public static void SetInfoPanel(this UIBase uiBase, ICommonInfoPanel panel)
	{
		if (!(uiBase == null) && panel != null)
		{
			InfoModule module = uiBase.GetModule<InfoModule>();
			if (module != null)
			{
				module.infoPanel = panel;
			}
		}
	}
}
