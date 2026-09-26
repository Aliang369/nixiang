public class zbLevelUpHandler
{
	private int m_nLastRoleLevel;

	private int m_nLastFightValue;

	private int m_nLastVipLevel;

	public void Init()
	{
		m_nLastRoleLevel = zbServerData.sGameData.RoleBaseInfo.Level;
		m_nLastFightValue = zbServerData.sGameData.RoleBaseInfo.TeamAttack;
		m_nLastVipLevel = zbServerData.sGameData.RoleBaseInfo.VipLevel;
	}

	public void Update()
	{
		if (!zbGameStateMachine.Instance.IsFighting() && !zbUIMgr.Instance.IsShow(EWndId.Combat) && !zbUIMgr.Instance.IsShow(EWndId.CombatBalance))
		{
			int level = zbServerData.sGameData.RoleBaseInfo.Level;
			int teamAttack = zbServerData.sGameData.RoleBaseInfo.TeamAttack;
			int vipLevel = zbServerData.sGameData.RoleBaseInfo.VipLevel;
			if (level > m_nLastRoleLevel)
			{
				m_nLastRoleLevel = level;
				zbUIMgr.Instance.ShowLevelUp();
			}
			if (teamAttack > m_nLastFightValue)
			{
				zbUIMgr.Instance.ShowFightValueUp(m_nLastFightValue);
			}
			if (vipLevel > m_nLastVipLevel)
			{
				m_nLastVipLevel = vipLevel;
				zbUIMgr.Instance.ShowVipLevelUpWnd();
			}
			m_nLastFightValue = teamAttack;
		}
	}
}
