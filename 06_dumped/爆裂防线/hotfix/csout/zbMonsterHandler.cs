using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterHandler
{
	private zbHandbookCsvTableData m_records;

	public void Init()
	{
		m_records = zbDataConfig.Instance.handbookCsvTable.GetRecords();
	}

	public bool IsShowBtnLeftAndRight(int type)
	{
		List<zbHandbookCsvRecord> recordsByType = GetRecordsByType(type);
		RepeatedField<MonsterHandbookDetail> monsterList = zbServerData.sGameData.HandbookInfo.MonsterList;
		HashSet<int> serverIds = new HashSet<int>(((IEnumerable<MonsterHandbookDetail>)monsterList).Select((MonsterHandbookDetail s) => s.HandbookId));
		return recordsByType.Where((zbHandbookCsvRecord r) => serverIds.Contains(r.id)).ToList().Count > 1;
	}

	public MonsterHandbookDetail GetPreMonsterDetailByRecord(zbHandbookCsvRecord record)
	{
		List<zbHandbookCsvRecord> recordsByType = GetRecordsByType(record.type);
		RepeatedField<MonsterHandbookDetail> monsterList = zbServerData.sGameData.HandbookInfo.MonsterList;
		HashSet<int> serverIds = new HashSet<int>(((IEnumerable<MonsterHandbookDetail>)monsterList).Select((MonsterHandbookDetail s) => s.HandbookId));
		List<zbHandbookCsvRecord> list = (from r in recordsByType
			where serverIds.Contains(r.id)
			orderby r.id
			select r).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort((zbHandbookCsvRecord a, zbHandbookCsvRecord b) => a.index.CompareTo(b.index));
		int num = list.FindIndex((zbHandbookCsvRecord r) => r.id == record.id);
		if (num > 0)
		{
			int preId = list[num - 1].id;
			return ((IEnumerable<MonsterHandbookDetail>)monsterList).FirstOrDefault((MonsterHandbookDetail s) => s.HandbookId == preId);
		}
		int lastId = list.Last().id;
		return ((IEnumerable<MonsterHandbookDetail>)monsterList).FirstOrDefault((MonsterHandbookDetail s) => s.HandbookId == lastId);
	}

	public MonsterHandbookDetail GetNextMonsterDetailByRecord(zbHandbookCsvRecord record)
	{
		List<zbHandbookCsvRecord> recordsByType = GetRecordsByType(record.type);
		RepeatedField<MonsterHandbookDetail> monsterList = zbServerData.sGameData.HandbookInfo.MonsterList;
		HashSet<int> serverIds = new HashSet<int>(((IEnumerable<MonsterHandbookDetail>)monsterList).Select((MonsterHandbookDetail s) => s.HandbookId));
		List<zbHandbookCsvRecord> list = (from r in recordsByType
			where serverIds.Contains(r.id)
			orderby r.id
			select r).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort((zbHandbookCsvRecord a, zbHandbookCsvRecord b) => a.index.CompareTo(b.index));
		int num = list.FindIndex((zbHandbookCsvRecord r) => r.id == record.id);
		if (num >= 0 && num < list.Count - 1)
		{
			int nextId = list[num + 1].id;
			return ((IEnumerable<MonsterHandbookDetail>)monsterList).FirstOrDefault((MonsterHandbookDetail s) => s.HandbookId == nextId);
		}
		int firstId = list.First().id;
		return ((IEnumerable<MonsterHandbookDetail>)monsterList).FirstOrDefault((MonsterHandbookDetail s) => s.HandbookId == firstId);
	}

	public List<zbHandbookCsvRecord> GetRecordsByType(int type)
	{
		List<zbHandbookCsvRecord> list = new List<zbHandbookCsvRecord>();
		foreach (zbHandbookCsvRecord value in m_records.records.Values)
		{
			if (value.type == type)
			{
				list.Add(value);
			}
		}
		list.Sort((zbHandbookCsvRecord a, zbHandbookCsvRecord b) => a.index.CompareTo(b.index));
		return list;
	}

	public void OnShowMonsterPreview(ClickEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Collider2D val2 = Physics2D.OverlapPoint(new Vector2(val.x, val.y));
		if (!((Object)(object)val2 != (Object)null))
		{
			return;
		}
		zbMonsterPreview component = ((Component)val2).gameObject.GetComponent<zbMonsterPreview>();
		if ((Object)(object)component != (Object)null)
		{
			zbMonsterCsvRecord monsterRecord = component.GetMonsterRecord();
			if (monsterRecord != null)
			{
				zbUIMgr.Instance.ShowPreviewMonsterDetail(monsterRecord);
			}
		}
	}
}
