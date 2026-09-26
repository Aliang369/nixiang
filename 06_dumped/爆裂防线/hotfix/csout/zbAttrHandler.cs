using UnityEngine;

public class zbAttrHandler
{
	private bool m_bFirstBuild;

	private zbAttrTeam m_attrTeam;

	private zbAttrWarRoom m_attrWarRoom;

	private float m_dataChangeLastTime;

	public void Init()
	{
		m_bFirstBuild = false;
		m_attrTeam = new zbAttrTeam();
		m_attrWarRoom = new zbAttrWarRoom();
		m_attrTeam.Init();
		m_attrWarRoom.Init();
		m_dataChangeLastTime = Time.unscaledTime;
	}

	public void OnLoadingComplete()
	{
		m_bFirstBuild = true;
		m_attrTeam.Build();
		m_attrWarRoom.Build();
	}

	public void DataChangeTeam()
	{
		if (m_bFirstBuild && m_attrTeam.DataChange())
		{
			m_dataChangeLastTime = Time.unscaledTime;
		}
	}

	public void DataChangeWarRoom()
	{
		if (m_bFirstBuild)
		{
			if (Time.unscaledTime - m_dataChangeLastTime > 1f)
			{
				m_attrWarRoom.DataChange();
			}
			else
			{
				m_attrWarRoom.Build();
			}
		}
	}
}
