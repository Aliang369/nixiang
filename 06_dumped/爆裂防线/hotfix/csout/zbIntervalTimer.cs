using UnityEngine;

public class zbIntervalTimer
{
	private double m_last1SecTime;

	private double m_last5SecTime;

	private double m_last10SecTime;

	private bool m_oneSecondPassed;

	private bool m_fiveSecondPassed;

	private bool m_tenSecondPassed;

	private static zbIntervalTimer sInstance;

	public static zbIntervalTimer Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbIntervalTimer();
			}
			return sInstance;
		}
	}

	public zbIntervalTimer()
	{
		m_last10SecTime = (m_last5SecTime = (m_last1SecTime = GetTime()));
	}

	public void Update()
	{
		m_oneSecondPassed = UpdateOneSecondPassed();
		m_fiveSecondPassed = UpdateFiveSecondPassed();
		m_tenSecondPassed = UpdateTenSecondPassed();
	}

	public bool IsOneSecondPassed()
	{
		return m_oneSecondPassed;
	}

	public bool IsFiveSecondPassed()
	{
		return m_fiveSecondPassed;
	}

	public bool IsTenSecondPassed()
	{
		return m_tenSecondPassed;
	}

	private bool UpdateOneSecondPassed()
	{
		double time = GetTime();
		if (time - m_last1SecTime >= 1.0)
		{
			m_last1SecTime = time;
			return true;
		}
		return false;
	}

	public bool UpdateFiveSecondPassed()
	{
		double time = GetTime();
		if (time - m_last5SecTime >= 5.0)
		{
			m_last5SecTime = time;
			return true;
		}
		return false;
	}

	public bool UpdateTenSecondPassed()
	{
		double time = GetTime();
		if (time - m_last10SecTime >= 10.0)
		{
			m_last10SecTime = time;
			return true;
		}
		return false;
	}

	private double GetTime()
	{
		return Time.unscaledTime;
	}
}
