public class zbMonsterHud
{
	private zbCombatData m_combatData;

	public zbMonsterHudPool m_hudPool;

	public void Init(zbCombatData combatData)
	{
		m_combatData = combatData;
		m_hudPool = new zbMonsterHudPool();
		m_hudPool.Init(m_combatData);
	}

	public void Free()
	{
		m_hudPool.Free();
	}

	public zbMonsterHudProgress ShowHud(zbMonster owner)
	{
		zbMonsterHudProgress hud = m_hudPool.GetHud();
		hud?.Show(m_combatData, owner);
		return hud;
	}

	public void ReturnHud(zbMonsterHudProgress hud)
	{
		m_hudPool.ReturnHud(hud);
	}

	public void Update()
	{
		m_hudPool.Update();
	}
}
