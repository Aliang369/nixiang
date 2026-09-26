using UnityEngine.UIElements;

public class zbSeasonForceTechTabOffice
{
	private zbSeasonForceTechWnd m_wndParent;

	private VisualElement m_RootElement;

	public void OnEnable(zbSeasonForceTechWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
	}

	public void Refresh()
	{
	}
}
