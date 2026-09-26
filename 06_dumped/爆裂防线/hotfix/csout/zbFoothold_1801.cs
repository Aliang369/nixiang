using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_1801 : zbFootholdBase
{
	public stFootholdIndex1801 footholdIndex1801Data = new stFootholdIndex1801();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbFootholdIndex1801[] footholdIndex1801;

	private List<int> pool = new List<int>();

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		_ = (zbFootholdIndex1801_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1801_2.prefab", this, new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY), Vector2.up);
		footholdIndex1801 = new zbFootholdIndex1801[footholdIndex1801Data.triggerNumber];
		for (int i = 0; i < footholdIndex1801Data.triggerNumber; i++)
		{
			footholdIndex1801[i] = (zbFootholdIndex1801)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1801.prefab", this, new Vector3(-100f, -100f), Vector2.up);
			footholdIndex1801[i].SetFootholdAtr(footholdIndex1801Data, footholdEffectRecord);
		}
		for (int j = 0; j < 84; j++)
		{
			pool.Add(j);
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1801Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1801Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1801Data.delayTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1801Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1801Data.buffId = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1801Data.buffTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1801Data.moveRange = footholdEffectRecord.param[num] / 100f;
		num++;
		m_damageInterval = footholdIndex1801Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex1801Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			TriggerDamage();
		}
	}

	private void TriggerDamage()
	{
		for (int num = pool.Count - 1; num > 0; num--)
		{
			int num2 = Rand.Next(num + 1);
			List<int> list = pool;
			int index = num;
			List<int> list2 = pool;
			int index2 = num2;
			int num3 = pool[num2];
			int num4 = pool[num];
			int num5 = (list[index] = num3);
			num5 = (list2[index2] = num4);
		}
		List<int> range = pool.GetRange(0, footholdIndex1801Data.triggerNumber);
		int num8 = 0;
		foreach (int item in range)
		{
			footholdIndex1801[num8].TriggerDamage(item);
			num8++;
		}
	}
}
