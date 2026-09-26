using System.Collections.Generic;
using UnityEngine;

public class zbCombatStat
{
	public Dictionary<int, long> heroDamageStats;

	public long defenseDamageStats;

	public long damageSum;

	public long damageBossSum;

	public zbCombatStat()
	{
		heroDamageStats = new Dictionary<int, long>();
	}

	public void Init()
	{
		heroDamageStats.Clear();
		damageSum = 0L;
		damageBossSum = 0L;
	}

	public void Free()
	{
		heroDamageStats.Clear();
		damageSum = 0L;
	}

	public void AddDamageStat(int heroTid, zbCombatDefense defense, long damage)
	{
		damageSum += damage;
		if ((Object)(object)defense == (Object)null)
		{
			if (heroDamageStats.TryGetValue(heroTid, out var value))
			{
				heroDamageStats[heroTid] = value + damage;
			}
			else
			{
				heroDamageStats[heroTid] = damage;
			}
		}
		else
		{
			defenseDamageStats += damage;
		}
	}

	public void AddBossDamageStat(long damage)
	{
		damageBossSum += damage;
	}

	public long GetHeroDamageStat(int heroTid)
	{
		if (heroDamageStats.ContainsKey(heroTid))
		{
			return heroDamageStats[heroTid];
		}
		return 0L;
	}

	public void Restore(List<stRecordHeroDamageStat> heroDamageStats, long damage)
	{
		foreach (stRecordHeroDamageStat heroDamageStat in heroDamageStats)
		{
			AddDamageStat(heroDamageStat.heroTid, null, heroDamageStat.damage);
		}
		damageBossSum += damage;
	}
}
