public class zbTacticItemUpgrade : zbViewBaseItem
{
	private zbTacticUpgradeWnd m_wndParent;

	private zbTacticUxmlCard m_uxmlCardLeft;

	private zbTacticUxmlCard m_uxmlCardRight;

	private zbTacticUxml4Stars m_uxmlStarsLeft;

	private zbTacticUxml4Stars m_uxmlStarsRight;

	public zbTacticItemUpgrade(zbTacticUpgradeWnd wndParent)
	{
		m_wndParent = wndParent;
	}
}
