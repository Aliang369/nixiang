using UnityEngine.UIElements;

public class zbSeasonSettlementTabRewardPreview
{
	private zbSeasonSettlementWnd m_wndParent;

	private VisualElement m_RootElement;

	public void OnEnable(zbSeasonSettlementWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
	}

	public void Refresh()
	{
		RefreshRankRange();
		RefreshMine();
	}

	private void RefreshRankRange()
	{
	}

	private void RefreshMine()
	{
	}
}
