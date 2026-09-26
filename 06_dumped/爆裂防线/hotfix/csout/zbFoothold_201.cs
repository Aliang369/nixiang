using UnityEngine;

public class zbFoothold_201 : zbFootholdBase
{
	public stFootholdIndex201 footholdIndex201Data = new stFootholdIndex201();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbFootholdIndex201[] footholdIndex201 = new zbFootholdIndex201[8];

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		for (int i = 0; i < 4; i++)
		{
			float num = 0f;
			int num2 = -5 + 3 * i;
			for (int j = 0; j < 2; j++)
			{
				num = 1.5f * (float)(j * 2 - 1) + combatData.nodeRootPositonX + Rand.Range(-0.5f, 0.5f);
				num2 += j * j;
				footholdIndex201[i + j * 4] = (zbFootholdIndex201)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex201.prefab", this, new Vector3(num, (float)num2), Vector2.up);
				footholdIndex201[i + j * 4].SetFootholdAtr(footholdIndex201Data, footholdEffectRecord);
			}
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex201Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.slowDownPrecent = footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.slowDownTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex201Data.freezeTime = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex201Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex201Data.skillEveryTime;
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
		foreach (zbFootholdIndex201 randomItem in Rand.GetRandomItems(footholdIndex201, footholdIndex201Data.triggerNumber))
		{
			randomItem.TriggerDamage();
		}
	}
}
