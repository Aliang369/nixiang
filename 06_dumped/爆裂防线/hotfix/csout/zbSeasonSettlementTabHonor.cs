using UnityEngine.UIElements;

public class zbSeasonSettlementTabHonor
{
	private zbSeasonSettlementWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_iconForce;

	private Label m_lbForceName;

	private Label m_lbRank;

	private VisualElement m_mine;

	private Label m_lbMineRank;

	private VisualElement m_minePlayer;

	private Label[] m_lbRankRanges = (Label[])(object)new Label[4];

	public void OnEnable(zbSeasonSettlementWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		for (int i = 0; i < 4; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "tier" + i, (string)null);
			m_lbRankRanges[i] = UQueryExtensions.Q<Label>(val, "lb_rankRange", (string)null);
		}
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_lbMineRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		m_minePlayer = UQueryExtensions.Q<VisualElement>(m_mine, "player", (string)null);
	}

	public void Refresh()
	{
	}
}
