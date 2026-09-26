using UnityEngine.UIElements;

public class zbSeasonRechargeTabZhanling
{
	private zbSeasonRechargeWnd m_wndParent;

	private VisualElement m_root;

	public void OnEnable(zbSeasonRechargeWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
	}

	public void Refresh()
	{
	}
}
