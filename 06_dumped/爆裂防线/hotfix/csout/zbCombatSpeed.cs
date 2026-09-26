using UnityEngine;
using zbType;

public class zbCombatSpeed
{
	private ECombatSpeed m_curSpeed = ECombatSpeed.X1;

	private string m_strConfig = "";

	private static readonly float speed1 = 1f;

	public static readonly float speed2 = 1.5f;

	public static readonly float speed3 = 2.5f;

	public ECombatSpeed GetSpeed()
	{
		return m_curSpeed;
	}

	public void Init()
	{
		m_strConfig = "combatSpeed_" + zbSelf.Instance.GetRoleId();
		switch (zbGameSave.Instance.GetInt(m_strConfig, 1))
		{
		case 3:
			m_curSpeed = ECombatSpeed.X3;
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.HighSpeed, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				m_curSpeed = ECombatSpeed.X2;
			}
			break;
		case 2:
			m_curSpeed = ECombatSpeed.X2;
			break;
		default:
			m_curSpeed = ECombatSpeed.X1;
			break;
		}
		SetSpeed(m_curSpeed);
	}

	public void SetSpeed(ECombatSpeed speed)
	{
		int value = 1;
		switch (speed)
		{
		case ECombatSpeed.X1:
			Time.timeScale = speed1;
			value = 1;
			break;
		case ECombatSpeed.X2:
			Time.timeScale = speed2;
			value = 2;
			break;
		case ECombatSpeed.X3:
			value = 3;
			Time.timeScale = speed3;
			break;
		}
		m_curSpeed = speed;
		zbGameSave.Instance.SetInt(m_strConfig, value);
	}

	public static float GetSpeedByEnum(ECombatSpeed eCombatSpeed)
	{
		return eCombatSpeed switch
		{
			ECombatSpeed.X3 => speed3, 
			ECombatSpeed.X2 => speed2, 
			_ => speed1, 
		};
	}
}
