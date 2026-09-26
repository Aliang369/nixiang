using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbLegionWarWatchMgr : MonoBehaviour
{
	public Transform heroTF;

	public static zbLegionWarWatchMgr Instance;

	public Dictionary<int, zbLegionWarWatchPoint> legionWarPointList { get; set; } = new Dictionary<int, zbLegionWarWatchPoint>();

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
		legionWarPointList = new Dictionary<int, zbLegionWarWatchPoint>();
	}

	public void Init(zbLegionWarWatchWnd _legionWarWnd)
	{
		if (legionWarPointList.Count > 0)
		{
			return;
		}
		foreach (KeyValuePair<int, zbLegionWarMapCsvRecord> record in zbDataConfig.Instance.legionWarMapCsvTable.GetRecords().records)
		{
			zbLegionWarWatchPoint component = ((Component)heroTF.Find("legionWarPoint" + record.Value.pointId)).GetComponent<zbLegionWarWatchPoint>();
			if (zbSelf.Instance.legionWarHandler.watchLegionWarForce == ELegionWarForce.Red)
			{
				legionWarPointList[record.Value.pointId] = component;
				component.Init(record.Value.pointId);
			}
			else
			{
				legionWarPointList[record.Value.mirrorId] = component;
				component.Init(record.Value.mirrorId);
			}
		}
	}

	public void Free()
	{
	}

	public void Refresh()
	{
		foreach (LegionWarPointStateDetail pointState in zbSelf.Instance.legionWarHandler.legionWarMapInfo.PointStateList)
		{
			legionWarPointList[pointState.PointId].Refresh(pointState);
		}
	}

	public zbLegionWarWatchPoint GetPoint(int pointId)
	{
		return legionWarPointList[pointId];
	}

	public void Update()
	{
		foreach (KeyValuePair<int, zbLegionWarWatchPoint> legionWarPoint in legionWarPointList)
		{
			legionWarPoint.Value.legionWarHud.Update();
		}
	}

	public bool IsNearPoint(int PointId, int myPoint)
	{
		return zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(PointId).connection.Contains(myPoint);
	}
}
