public class zbSeasonMgr
{
	private static zbSeasonMgr m_instance;

	private zbSeasonWnd m_wndParent;

	public zbSeasonStateMachine fsm;

	public static zbSeasonMgr Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new zbSeasonMgr();
			}
			return m_instance;
		}
	}

	public zbSeasonMap SeasonMap => m_wndParent.GetSeasonMap();

	public void OnEnable(zbSeasonWnd wndParent)
	{
		m_wndParent = wndParent;
		fsm = new zbSeasonStateMachine();
		fsm.Init();
		fsm.SetState(ESeasonState.Running);
	}

	public void SetUiElementVisible(bool visible)
	{
		m_wndParent.SetUiElementVisible(visible);
	}

	public void Update()
	{
		fsm.Update();
	}
}
