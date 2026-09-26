public class zbServerIndexData
{
	public bool m_bLastTab;

	public int m_Index;

	public int m_rem;

	public zbServerIndexData(int index, bool bLastTab, int rem = 0)
	{
		m_Index = index;
		m_bLastTab = bLastTab;
		m_rem = rem;
	}
}
