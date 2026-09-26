using System.Collections.Generic;
using UnityEngine;

public class zbZombieWave
{
	private float normalSpawnInterval = 3f;

	private float normalTimer;

	private float raidDuration = 3f;

	private float raidSpawnInterval = 0.2f;

	private float raidTimer;

	private bool isRaiding;

	private bool isInitialWave = true;

	private float initialTimer;

	private const float InitialWaveDuration = 2f;

	public void Init()
	{
		isInitialWave = true;
		initialTimer = 0f;
		SetZombieSpawnPosition();
	}

	private void SetZombieSpawnPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		zbRoomDefine.Instance.ZombieSpawnPosition = zbRoomDefine.Instance.ZombieTargetPosition + new Vector3((0f - initialTimer) * 4f - 2f, Rand.Range(-0.08f, 0.08f), 0f);
	}

	private void ResetZombieSpawnPosition()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		isInitialWave = false;
		zbRoomDefine.Instance.ZombieSpawnPosition = new Vector3(-18.5f, -13.41f, 0f);
	}

	public void Update()
	{
		if (isInitialWave)
		{
			initialTimer += Time.deltaTime;
			if (initialTimer < 2f)
			{
				TrySpawnZombie(ref raidSpawnInterval, ref normalTimer, 0.1f, 0.3f);
			}
			else
			{
				ResetZombieSpawnPosition();
			}
		}
		else if (isRaiding)
		{
			raidTimer += Time.deltaTime;
			if (raidTimer < raidDuration)
			{
				if (raidTimer < raidDuration - 0.2f)
				{
					TrySpawnZombie(ref raidSpawnInterval, ref normalTimer, 0.05f, 0.1f, isSweep: true);
				}
			}
			else
			{
				SetAllZombieDie();
				ResetZombieSpawnPosition();
				isRaiding = false;
				raidTimer = 0f;
			}
		}
		else
		{
			TrySpawnZombie(ref normalSpawnInterval, ref normalTimer, 2f, 6f);
		}
	}

	private void TrySpawnZombie(ref float interval, ref float timer, float randomMin, float randomMax, bool isSweep = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (isRaiding & isSweep)
		{
			zbRoomDefine.Instance.ZombieSpawnPosition = zbRoomDefine.Instance.ZombieTargetPosition + new Vector3(Rand.Range(-7f, -0f), Rand.Range(-0.08f, 0.08f), 0f);
		}
		timer += Time.deltaTime;
		if (timer >= interval)
		{
			timer = 0f;
			interval = Rand.Range(randomMin, randomMax);
			if (isInitialWave)
			{
				SetZombieSpawnPosition();
			}
			zbHomeMgr.Instance.zombieMgr.Spawn(isSweep);
		}
	}

	private void SetAllZombieDie()
	{
		foreach (KeyValuePair<int, zbZombie> zombie in zbHomeMgr.Instance.zombieMgr.zombieList)
		{
			zombie.Value.OnHit(killed: true);
		}
	}
}
