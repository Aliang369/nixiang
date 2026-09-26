using UnityEngine.UIElements;

public class zbSeasonRankTabForce
{
	private zbSeasonRankWnd m_wndParent;

	private VisualElement m_RootElement;

	public void OnEnable(zbSeasonRankWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
	}

	public void Refresh()
	{
	}
}
