using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;

public class zbFuncUnLockHandler
{
	public List<zbFunctionUnlockCsvRecord> m_funcUnlockRecords = new List<zbFunctionUnlockCsvRecord>();

	public void Init()
	{
	}

	private zbFunctionUnlockCsvRecord[] GetFuncUnLockRecords()
	{
		List<zbFunctionUnlockCsvRecord> list = new List<zbFunctionUnlockCsvRecord>();
		foreach (zbFunctionUnlockCsvRecord value in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records.Values)
		{
			if (value.unlockType == 1)
			{
				if (value.unlockVal <= 200)
				{
					list.Add(value);
				}
			}
			else if (value.unlockType == 2 && value.unlockVal <= zbDataConfig.Instance.peerageCsvTable.GetRecords().records.Keys.Max())
			{
				list.Add(value);
			}
		}
		return list.ToArray();
	}

	public void UpdateFunctionUnlockRecords(PeerageUpgradeAck ack)
	{
		m_funcUnlockRecords = GetFuntionUnlockRecords(ack.PeerageInfo.PeerageId);
	}

	private List<zbFunctionUnlockCsvRecord> GetFuntionUnlockRecords(int peerageId)
	{
		List<zbFunctionUnlockCsvRecord> list = new List<zbFunctionUnlockCsvRecord>();
		zbFunctionUnlockCsvRecord[] funcUnLockRecords = GetFuncUnLockRecords();
		foreach (zbFunctionUnlockCsvRecord zbFunctionUnlockCsvRecord2 in funcUnLockRecords)
		{
			if (zbFunctionUnlockCsvRecord2.unlockType == 2 && zbFunctionUnlockCsvRecord2.unlockVal == peerageId)
			{
				if (zbFunctionUnlockCsvRecord2.showTime == 1)
				{
					list.Add(zbFunctionUnlockCsvRecord2);
				}
				zbSelf.Instance.newTagHandler.AddNewFunc(zbFunctionUnlockCsvRecord2.id.ToString());
			}
		}
		return list;
	}

	public zbFunctionUnlockCsvRecord GetLastRecord()
	{
		zbFunctionUnlockCsvRecord[] funcUnLockRecords = GetFuncUnLockRecords();
		zbFunctionUnlockCsvRecord result = null;
		int num = int.MinValue;
		zbFunctionUnlockCsvRecord[] array = funcUnLockRecords;
		foreach (zbFunctionUnlockCsvRecord zbFunctionUnlockCsvRecord2 in array)
		{
			if (zbFunctionUnlockCsvRecord2.unlockType == 2 && zbFunctionUnlockCsvRecord2.showTime == 1)
			{
				int unlockVal = zbFunctionUnlockCsvRecord2.unlockVal;
				if (unlockVal > num)
				{
					num = unlockVal;
					result = zbFunctionUnlockCsvRecord2;
				}
			}
		}
		return result;
	}

	public zbFunctionUnlockCsvRecord GetNextRecordByPeerageId(int peerageId)
	{
		zbFunctionUnlockCsvRecord[] funcUnLockRecords = GetFuncUnLockRecords();
		zbFunctionUnlockCsvRecord result = null;
		int num = int.MaxValue;
		zbFunctionUnlockCsvRecord[] array = funcUnLockRecords;
		foreach (zbFunctionUnlockCsvRecord zbFunctionUnlockCsvRecord2 in array)
		{
			if (zbFunctionUnlockCsvRecord2 != null && zbFunctionUnlockCsvRecord2.unlockType == 2)
			{
				int unlockVal = zbFunctionUnlockCsvRecord2.unlockVal;
				if (unlockVal > peerageId && unlockVal < num)
				{
					num = unlockVal;
					result = zbFunctionUnlockCsvRecord2;
				}
			}
		}
		return result;
	}

	public List<zbFunctionUnlockCsvRecord> GetRecordsByPeerageId(int peerageId)
	{
		List<zbFunctionUnlockCsvRecord> list = (from r in GetFuncUnLockRecords()
			where r.unlockType == 2 && r.showTime == 1
			orderby r.unlockVal
			select r).ToList();
		List<zbFunctionUnlockCsvRecord> list2 = new List<zbFunctionUnlockCsvRecord>();
		List<zbFunctionUnlockCsvRecord> list3 = new List<zbFunctionUnlockCsvRecord>();
		List<zbFunctionUnlockCsvRecord> list4 = new List<zbFunctionUnlockCsvRecord>();
		foreach (zbFunctionUnlockCsvRecord item in list)
		{
			bool num = item.unlockVal <= peerageId;
			bool flag = num && zbServerData.sGameData.AuxAwardInfo.FuncList.Contains(item.id);
			if (!num)
			{
				list3.Add(item);
			}
			else if (flag)
			{
				list4.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		List<zbFunctionUnlockCsvRecord> list5 = new List<zbFunctionUnlockCsvRecord>();
		list5.AddRange(list2);
		list5.AddRange(list3.Take(2));
		list5.AddRange(list4);
		return list5;
	}

	public bool HasRedPoint()
	{
		RepeatedField<int> funcList = zbServerData.sGameData.AuxAwardInfo.FuncList;
		List<int> list = new List<int>();
		zbFunctionUnlockCsvRecord[] funcUnLockRecords = GetFuncUnLockRecords();
		foreach (zbFunctionUnlockCsvRecord zbFunctionUnlockCsvRecord2 in funcUnLockRecords)
		{
			if (zbFunctionUnlockCsvRecord2.unlockType == 2 && zbFunctionUnlockCsvRecord2.unlockVal <= zbServerData.sGameData.PeerageInfo.PeerageId && zbFunctionUnlockCsvRecord2.showTime == 1)
			{
				list.Add(zbFunctionUnlockCsvRecord2.id);
			}
		}
		List<int> list2 = new List<int>();
		foreach (int item in list)
		{
			if (!funcList.Contains(item) && !zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)item))
			{
				list2.Add(item);
			}
		}
		return list2.Count > 0;
	}

	public void Update()
	{
		if (m_funcUnlockRecords.Count > 0 && zbUIMgr.Instance.IsShow(EWndId.MainLevel) && !zbUIMgr.Instance.IsShow(EWndId.FuncUnLockAward))
		{
			zbUIMgr.Instance.ShowFuncUnLockAwardWnd((EFuncType)m_funcUnlockRecords[0].id);
			zbSelf.Instance.funcUnLockHandler.m_funcUnlockRecords.RemoveAt(0);
		}
	}
}
