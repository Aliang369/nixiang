using System.Collections.Generic;
using UnityEngine;

public class zbZombieMgr
{
	public zbZombiePool zombiePool;

	public zbZombieWave zombieWave;

	public Dictionary<int, zbZombie> zombieList = new Dictionary<int, zbZombie>();

	public List<zbZombie> cachedSortedZombieList = new List<zbZombie>();

	public void Init(GameObject objScene)
	{
		zombiePool = new zbZombiePool();
		zombiePool.Init(objScene);
		zombieWave = new zbZombieWave();
		zombieWave.Init();
	}

	public void Free()
	{
	}

	public void Build()
	{
	}

	public void Update()
	{
		zombieWave.Update();
	}

	public void Spawn(bool isSweep)
	{
		zombiePool.Spawn().Init(isSweep);
	}

	public void Recycle(zbZombie zombie)
	{
		zombiePool.Recycle(zombie);
	}

	public void AddZombie(zbZombie zombie)
	{
		int uUID = zombie.GetUUID();
		if (!zombieList.ContainsKey(uUID))
		{
			zombieList[uUID] = zombie;
		}
	}

	public void RemoveZombie(zbZombie zombie)
	{
		zombieList.Remove(zombie.GetUUID());
	}

	public zbZombie GetZombieById(int uuid)
	{
		zombieList.TryGetValue(uuid, out var value);
		return value;
	}

	public List<zbZombie> GetSortedZombiesInAttackRange(float attackRangeSqr)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		cachedSortedZombieList.Clear();
		Vector3 turretPos = ((Component)zbHomeMgr.Instance.paotaiMgr.paotai).transform.position;
		foreach (KeyValuePair<int, zbZombie> zombie in zombieList)
		{
			zbZombie value = zombie.Value;
			if (!value.IsDie())
			{
				Vector2 val = Vector2.op_Implicit(((Component)value).transform.position - turretPos);
				if (((Vector2)(ref val)).sqrMagnitude <= attackRangeSqr)
				{
					cachedSortedZombieList.Add(value);
				}
			}
		}
		cachedSortedZombieList.Sort(delegate(zbZombie a, zbZombie b)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.SqrMagnitude(((Component)a).transform.position - turretPos);
			float value2 = Vector3.SqrMagnitude(((Component)b).transform.position - turretPos);
			return num.CompareTo(value2);
		});
		return cachedSortedZombieList;
	}

	public zbZombie GetNearestZombieToWall()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		zbZombie result = null;
		Vector3 position = ((Component)zbHomeMgr.Instance.paotaiMgr.paotai).transform.position;
		foreach (KeyValuePair<int, zbZombie> zombie in zombieList)
		{
			zbZombie value = zombie.Value;
			if (!value.IsDie())
			{
				float num2 = Vector3.SqrMagnitude(((Component)value).transform.position - position);
				if (num2 < num)
				{
					num = num2;
					result = value;
				}
			}
		}
		return result;
	}
}
