public class zbSetSave
{
	private static zbSetSave _instance;

	private float m_MaxDamage = 1f;

	private float m_Maxeffects = 1f;

	private int m_Consumption;

	private int m_Vibration;

	private int m_NonFriend;

	private int m_GemCast;

	private int m_SnowVeilRolling;

	private int m_SnowVeilTurntable;

	private int m_SnowVeilSpeed;

	private bool m_WorldPanelOpen;

	private int m_FacilityRolling;

	private string m_strConfig1;

	private string m_strConfig2;

	private string m_strConfig3;

	private string m_strConfig5;

	private string m_strConfig6;

	private string m_strConfig7;

	private string m_strConfig8;

	private string m_strConfig9;

	private string m_strConfig10;

	private string m_strConfig11;

	private string m_strConfig12;

	public static zbSetSave Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new zbSetSave();
				_instance.Init();
			}
			return _instance;
		}
	}

	private zbSetSave()
	{
		string text = zbSelf.Instance.GetRoleId().ToString();
		m_strConfig1 = "MaxDamage" + text;
		m_strConfig2 = "Maxeffects" + text;
		m_strConfig3 = "Consumption" + text;
		m_strConfig5 = "Vibration" + text;
		m_strConfig6 = "NonFriend" + text;
		m_strConfig7 = "GemCast" + text;
		m_strConfig8 = "WorldPanelOpen" + text;
		m_strConfig9 = "SnowVeilRolling" + text;
		m_strConfig10 = "SnowVeilTurntable" + text;
		m_strConfig11 = "SnowVeilSpeed" + text;
		m_strConfig12 = "FacilityRolling" + text;
	}

	public float GetMaxDamage()
	{
		return m_MaxDamage;
	}

	public float GetMaxeffects()
	{
		return m_Maxeffects;
	}

	public int GetConsumption()
	{
		return m_Consumption;
	}

	public int GetVibration()
	{
		return m_Vibration;
	}

	public int GetNonFriend()
	{
		return m_NonFriend;
	}

	public int GetGemCast()
	{
		return m_GemCast;
	}

	public int GetSnowVeilRolling()
	{
		return m_SnowVeilRolling;
	}

	public int GetSnowVeilTurntable()
	{
		return m_SnowVeilTurntable;
	}

	public int GetSnowVeilSpeed()
	{
		return m_SnowVeilSpeed;
	}

	public bool GetWorldPanelOpen()
	{
		return m_WorldPanelOpen;
	}

	public int GetFacilityRolling()
	{
		return m_FacilityRolling;
	}

	private void Init()
	{
		m_MaxDamage = zbGameSave.Instance.GetFloat(m_strConfig1, m_MaxDamage);
		m_Maxeffects = zbGameSave.Instance.GetFloat(m_strConfig2, m_Maxeffects);
		m_Consumption = zbGameSave.Instance.GetInt(m_strConfig3, m_Consumption);
		m_Vibration = zbGameSave.Instance.GetInt(m_strConfig5, m_Vibration);
		m_NonFriend = zbGameSave.Instance.GetInt(m_strConfig6, m_NonFriend);
		m_GemCast = zbGameSave.Instance.GetInt(m_strConfig7, m_GemCast);
		m_WorldPanelOpen = zbGameSave.Instance.GetBool(m_strConfig8, m_WorldPanelOpen);
		m_SnowVeilRolling = zbGameSave.Instance.GetInt(m_strConfig9, m_SnowVeilRolling);
		m_SnowVeilTurntable = zbGameSave.Instance.GetInt(m_strConfig10, m_SnowVeilTurntable);
		m_SnowVeilSpeed = zbGameSave.Instance.GetInt(m_strConfig11, m_SnowVeilSpeed);
		m_FacilityRolling = zbGameSave.Instance.GetInt(m_strConfig12, m_FacilityRolling);
	}

	public void SetConsumption(int val)
	{
		m_Consumption = val;
		zbGameSave.Instance.SetInt(m_strConfig3, val);
	}

	public void SetVibration(int val)
	{
		m_Vibration = val;
		zbGameSave.Instance.SetInt(m_strConfig5, val);
	}

	public void SetNonFriend(int val)
	{
		m_NonFriend = val;
		zbGameSave.Instance.SetInt(m_strConfig6, val);
	}

	public void SetGemCast(int val)
	{
		m_GemCast = val;
		zbGameSave.Instance.SetInt(m_strConfig7, val);
	}

	public void SetSnowVeilRolling(int val)
	{
		m_SnowVeilRolling = val;
		zbGameSave.Instance.SetInt(m_strConfig9, val);
	}

	public void SetSnowVeilTurntable(int val)
	{
		m_SnowVeilTurntable = val;
		zbGameSave.Instance.SetInt(m_strConfig10, val);
	}

	public void SetSnowVeilSpeed(int val)
	{
		m_SnowVeilSpeed = val;
		zbGameSave.Instance.SetInt(m_strConfig11, val);
	}

	public void SetFacilityRolling(int val)
	{
		m_FacilityRolling = val;
		zbGameSave.Instance.SetInt(m_strConfig12, val);
	}

	public void SetWorldPanelOpen(bool val)
	{
		m_WorldPanelOpen = val;
		zbGameSave.Instance.SetBool(m_strConfig8, val);
	}
}
