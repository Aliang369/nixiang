using System.Collections.Generic;
using Best.HTTP.Shared.Extensions;
using Proto.Enums;
using zbType;

public class zbProgressEventHelper
{
	private static zbProgressEventHelper sInstance;

	public static zbProgressEventHelper Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbProgressEventHelper();
			}
			return sInstance;
		}
	}

	public void Init()
	{
	}

	public void SetProgressEvent1(int messageId)
	{
		foreach (KeyValuePair<int, zbProgressEventCsvRecord> record in zbDataConfig.Instance.progressEventCsvTable.GetRecords().records)
		{
			if (record.Value.firstType == 1 && (messageId == Extensions.ToInt32(record.Value.firstSubType1, 0) || messageId == record.Value.firstSubType2))
			{
				SetProgressTrack(record.Value.id);
				break;
			}
		}
	}

	public void SetProgressEvent2(EWndId wndId, int tabId = 0)
	{
		foreach (KeyValuePair<int, zbProgressEventCsvRecord> record in zbDataConfig.Instance.progressEventCsvTable.GetRecords().records)
		{
			if (record.Value.firstType == 2 && wndId.ToString() == record.Value.firstSubType1 && tabId == record.Value.firstSubType2)
			{
				SetProgressTrack(record.Value.id);
				break;
			}
		}
	}

	public void SetProgressEvent4(int PeerageId)
	{
		foreach (KeyValuePair<int, zbProgressEventCsvRecord> record in zbDataConfig.Instance.progressEventCsvTable.GetRecords().records)
		{
			if (record.Value.firstType == 4 && PeerageId == Extensions.ToInt32(record.Value.firstSubType1, 0))
			{
				SetProgressTrack(record.Value.id);
				break;
			}
		}
	}

	public void SetProgressEvent10(int EnemyId)
	{
		foreach (KeyValuePair<int, zbProgressEventCsvRecord> record in zbDataConfig.Instance.progressEventCsvTable.GetRecords().records)
		{
			if (record.Value.firstType == 10 && EnemyId == Extensions.ToInt32(record.Value.firstSubType1, 0))
			{
				SetProgressTrack(record.Value.id);
				break;
			}
		}
	}

	public void SetProgressTrack(int EventType)
	{
		if (zbLoginUtil.appChannel != EChannelType.ZbRobot && zbServerData.sGameData != null && !zbServerData.sGameData.ProgressEventList.Contains(EventType))
		{
			zbWsMsgHandler.Instance.Aux_ProgressTrack_Req(EventType);
			zbProgressEventCsvRecord recordByKey = zbDataConfig.Instance.progressEventCsvTable.GetRecordByKey(EventType);
			if (recordByKey != null)
			{
				zbChannel_Report.ReportDataByChannel(EReportType.E_CUSTOM_EVENT, recordByKey.eventStr);
			}
		}
	}
}
