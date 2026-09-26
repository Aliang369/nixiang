using System;
using System.Collections.Generic;
using zbType;

public class zbRedPointHandler
{
	private Dictionary<ERedPointType, int> m_stateList = new Dictionary<ERedPointType, int>();

	public void Init()
	{
		foreach (ERedPointType value in Enum.GetValues(typeof(ERedPointType)))
		{
			m_stateList[value] = 0;
		}
	}

	public void SetStateValue(ERedPointType eRedType, bool eValue)
	{
		if (eValue)
		{
			m_stateList[eRedType] = 1;
		}
		else
		{
			m_stateList[eRedType] = 0;
		}
	}

	public bool HasRedPoint(ERedPointType eRedType)
	{
		return m_stateList[eRedType] == 1;
	}
}
