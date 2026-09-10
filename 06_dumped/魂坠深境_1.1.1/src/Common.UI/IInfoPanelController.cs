namespace Common.UI;

public interface IInfoPanelController
{
	bool HasShowingPanel();

	void OnHideAllInfo();

	UINode GetUINode();
}
