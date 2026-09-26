using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbFunHandler
{
	private int m_nLastPeerage;

	private Dictionary<int, long> m_funcOpenTimes = new Dictionary<int, long>();

	public void Init()
	{
		m_nLastPeerage = zbServerData.sGameData.PeerageInfo.PeerageId;
	}

	public bool IsLockAndTip(EFuncType eType)
	{
		return IsLock(eType, bPopTip: true);
	}

	public bool IsLockNoTip(EFuncType eType)
	{
		return IsLock(eType, bPopTip: false);
	}

	public bool IsLock(EFuncType eType, bool bPopTip)
	{
		if (eType == EFuncType.ElementTrial && zbSelf.Instance.elementTrialHandler.IsElementTrialOpened())
		{
			return false;
		}
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)eType);
		if (recordByKey == null)
		{
			return true;
		}
		int num;
		switch (recordByKey.unlockType)
		{
		case 1:
			num = zbServerData.sGameData.RoleBaseInfo.Level;
			break;
		case 2:
			num = zbServerData.sGameData.PeerageInfo.PeerageId;
			break;
		default:
			return true;
		}
		bool flag = num < recordByKey.unlockVal;
		long funcOpenLeftTime = GetFuncOpenLeftTime(recordByKey.serverHours);
		if (bPopTip)
		{
			if (flag)
			{
				zbUIMgr.Instance.BubbleInfo(GetFunctionUnlockDesc((int)eType));
			}
			else if (funcOpenLeftTime > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10886"), zbUtil.FormatTimeSmartFormatTimeToHMS(funcOpenLeftTime)));
			}
		}
		if (!flag)
		{
			return funcOpenLeftTime > 0;
		}
		return true;
	}

	public long GetFuncOpenTm(int hours)
	{
		RoleCommonInfo roleCommonInfo = zbServerData.sGameData.RoleCommonInfo;
		if (roleCommonInfo == null)
		{
			return -1L;
		}
		return roleCommonInfo.ServerOpenTime + (long)hours * 3600L;
	}

	public long GetFuncOpenLeftTime(int hours)
	{
		if (!m_funcOpenTimes.TryGetValue(hours, out var value))
		{
			value = GetFuncOpenTm(hours);
			m_funcOpenTimes[hours] = value;
		}
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		return value - serverTime;
	}

	public bool IsNeedGuide(EFuncType eType)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(eType))
		{
			return false;
		}
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)eType);
		if (recordByKey != null)
		{
			int num = 0;
			switch (recordByKey.unlockType)
			{
			case 1:
				num = zbServerData.sGameData.RoleBaseInfo.Level;
				break;
			case 2:
				num = zbServerData.sGameData.PeerageInfo.PeerageId;
				break;
			default:
				return false;
			}
			if (num != recordByKey.unlockVal)
			{
				return num == recordByKey.unlockVal + 1;
			}
			return true;
		}
		return false;
	}

	public void Update()
	{
		int peerageId = zbServerData.sGameData.PeerageInfo.PeerageId;
		if (peerageId > m_nLastPeerage)
		{
			m_nLastPeerage = peerageId;
			CheckNewFunc();
		}
	}

	public void CheckNewFunc(bool isLevelUp = false)
	{
		zbFunctionUnlockCsvRecord functionUnlockRecord = GetFunctionUnlockRecord(isLevelUp);
		if (functionUnlockRecord != null && functionUnlockRecord.showTime == 0)
		{
			zbUIMgr.Instance.ShowFunc(functionUnlockRecord);
		}
	}

	private zbFunctionUnlockCsvRecord GetFunctionUnlockRecord(bool isLevelUp = false)
	{
		foreach (KeyValuePair<int, zbFunctionUnlockCsvRecord> record in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records)
		{
			if (isLevelUp && record.Value.unlockType == 1 && record.Value.unlockVal == zbServerData.sGameData.RoleBaseInfo.Level)
			{
				return record.Value;
			}
			if (!isLevelUp && record.Value.unlockType == 2 && record.Value.unlockVal == m_nLastPeerage)
			{
				return record.Value;
			}
		}
		return null;
	}

	private string GetFunctionUnlockDesc(int id)
	{
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			string obj = ((recordByKey.unlockType == 1) ? zbUtil.Language("lang.10289") : zbUtil.Language("lang.10290"));
			string text = "";
			if (recordByKey.unlockType == 1)
			{
				text = recordByKey.unlockVal.ToString();
			}
			else
			{
				zbPeerageCsvRecord recordByKey2 = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(recordByKey.unlockVal);
				text = ((recordByKey2 == null) ? recordByKey.unlockVal.ToString() : zbUtil.Language(recordByKey2.name));
			}
			return obj + text;
		}
		return "";
	}
}
