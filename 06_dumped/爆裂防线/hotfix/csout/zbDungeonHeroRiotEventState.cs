using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;

public class zbDungeonHeroRiotEventState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState) : zbDungeonHeroState(fsm, eState)
{
	private DungeonBaseInfo dungeonBaseInfo;

	private zbDungeonRoom m_room;

	private Coroutine m_curCoroutine;

	private List<int> npcAvatarList = new List<int>();

	private int m_zombieSpawnCount;

	public override void OnEnter()
	{
		base.OnEnter();
		StopAction();
		dungeonBaseInfo = zbDungeonMgr.Instance.dungeonHandler.dungeonBaseInfo;
		zbDungeonCsvRecord recordByKey = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(dungeonBaseInfo.DungeonId);
		if (recordByKey != null)
		{
			npcAvatarList.Clear();
			npcAvatarList = recordByKey.npcAvatarId.ToList();
		}
		m_room = m_fsm.hero.currentRoom;
		if (!((Object)(object)m_room == (Object)null))
		{
			CreateRiotZombies();
			m_curCoroutine = ((MonoBehaviour)m_fsm.hero).StartCoroutine(StartFight());
		}
	}

	private void CreateRiotZombies()
	{
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/dungeon/riotzombies.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		m_zombieSpawnCount = 0;
		bool flag = zbDungeonMgr.Instance.dungeonHandler.IsDoorOpen(m_room.dungeonMapCellDetail, isLeft: true);
		bool flag2 = zbDungeonMgr.Instance.dungeonHandler.IsDoorOpen(m_room.dungeonMapCellDetail, isLeft: false);
		if (flag & flag2)
		{
			for (int i = 0; i < 2; i++)
			{
				SpawnZombies(val, bLeft: true);
			}
			for (int j = 0; j < 2; j++)
			{
				SpawnZombies(val, bLeft: false);
			}
		}
		else if (flag)
		{
			for (int k = 0; k < 4; k++)
			{
				SpawnZombies(val, bLeft: true);
			}
		}
		else if (flag2)
		{
			for (int l = 0; l < 4; l++)
			{
				SpawnZombies(val, bLeft: false);
			}
		}
	}

	private void SpawnZombies(GameObject prefab, bool bLeft)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		float num = Rand.Range(1f, 3f);
		if (bLeft)
		{
			num = 0f - num;
		}
		Vector3 position = ((Component)m_fsm.hero).transform.position;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(position.x + num, position.y, position.z + (float)m_zombieSpawnCount * 0.1f);
		m_zombieSpawnCount++;
		GameObject obj = Object.Instantiate<GameObject>(prefab, val, Quaternion.identity);
		zbDungeonZombies component = obj.GetComponent<zbDungeonZombies>();
		if ((Object)(object)component != (Object)null)
		{
			int index = Rand.Range(0, npcAvatarList.Count);
			int npcAvatarId = npcAvatarList[index];
			component.RefreshSpine(npcAvatarId);
			component.SetTarget(((Component)m_fsm.hero).transform);
		}
		obj.transform.SetParent(((Component)m_room).transform, true);
	}

	public override void OnExit()
	{
		base.OnExit();
		StopAction();
		DestroyAllZombies();
	}

	private void StopAction()
	{
		if (m_curCoroutine != null)
		{
			((MonoBehaviour)m_fsm.hero).StopCoroutine(m_curCoroutine);
		}
	}

	private void DestroyAllZombies()
	{
		zbDungeonZombies[] componentsInChildren = ((Component)m_room).GetComponentsInChildren<zbDungeonZombies>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy((Object)(object)((Component)componentsInChildren[i]).gameObject);
		}
	}

	private IEnumerator StartFight()
	{
		yield return (object)new WaitForSeconds(1.5f);
		zbUIMgr.Instance.ShowDungeonFightPop(dungeonBaseInfo.TriggerEvent);
		m_fsm.SetState(EDungeonHeroState.Idle);
	}
}
