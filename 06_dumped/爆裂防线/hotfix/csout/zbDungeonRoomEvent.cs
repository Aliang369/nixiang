using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbDungeonRoomEvent
{
	private zbDungeonRoom m_room;

	public zbDungeonRoomEvent(zbDungeonRoom room)
	{
		m_room = room;
	}

	public void RefreshEvent()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		SkeletonAnimation[] spineEvent = m_room.spineEvent;
		for (int i = 0; i < spineEvent.Length; i++)
		{
			((Component)spineEvent[i]).gameObject.SetActive(false);
		}
		for (int j = 0; j < m_room.dungeonMapCellDetail.GridList.Count; j++)
		{
			DungeonMapGridDetail dungeonMapGridDetail = m_room.dungeonMapCellDetail.GridList[j];
			if (dungeonMapGridDetail == null || dungeonMapGridDetail.EventType == EDungeonEventType.EmptyValue || dungeonMapGridDetail.State == EActiveState.Done)
			{
				continue;
			}
			SkeletonAnimation val = m_room.spineEvent[j];
			if ((Object)(object)val != (Object)null)
			{
				if (dungeonMapGridDetail.EventType == EDungeonEventType.BossFight)
				{
					RefreshBossEvent(val);
				}
				else
				{
					RefreshOtherEvent(val, dungeonMapGridDetail);
				}
				float num = ((Component)m_room).transform.position.x + (float)j * 3f + 1.5f;
				((Component)val).transform.position = new Vector3(num, ((Component)val).transform.position.y, ((Component)val).transform.position.z);
				((Component)val).gameObject.SetActive(true);
			}
		}
	}

	private void RefreshBossEvent(SkeletonAnimation animation)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		int dungeonId = zbDungeonMgr.Instance.dungeonHandler.dungeonBaseInfo.DungeonId;
		zbDungeonCsvRecord recordByKey = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(dungeonId);
		if (recordByKey == null)
		{
			return;
		}
		zbMonsterCsvRecord recordByKey2 = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(recordByKey.mstBoss);
		if (recordByKey2 != null)
		{
			zbMonsterAvatarCsvRecord recordByKey3 = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey2.avatarId);
			if (recordByKey3 != null)
			{
				zbUtil.RefreshSpine(animation, recordByKey3.fightBone + "_SkeletonData.asset", "idle", loop: true);
				((Component)animation).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			}
		}
	}

	private void RefreshOtherEvent(SkeletonAnimation animation, DungeonMapGridDetail gridDetail)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		zbDungeonEventCsvRecord recordByKey = zbDataConfig.Instance.dungeonEventCsvTable.GetRecordByKey(gridDetail.EventId);
		if (recordByKey == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(recordByKey.eventSpine) && recordByKey.eventSpine != "0")
		{
			string spinePath = recordByKey.eventSpine + "_SkeletonData.asset";
			zbUtil.RefreshSpine(animation, spinePath, "1", loop: true);
			((Component)animation).transform.localScale = new Vector3(1f, 1f, 1f);
		}
		else if (recordByKey.npcAvatar != 0)
		{
			zbNpcAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.npcAvatarCsvTable.GetRecordByKey(recordByKey.npcAvatar);
			if (recordByKey2 != null)
			{
				zbStyleInfoHelper.RefreshNpcAvatar(animation, recordByKey2);
				((Component)animation).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			}
		}
	}
}
