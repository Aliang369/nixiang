using UnityEngine;

public class zbFoothold_1001 : zbFootholdBase
{
	public stFootholdIndex1001 footholdIndex1001Data = new stFootholdIndex1001();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private int[] metroIndex = new int[18]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17
	};

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1001Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1001Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1001Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex1001Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1001Data.burnBuffId = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1001Data.burnTime = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex1001Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex1001Data.skillEveryTime;
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
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		foreach (int randomItem in Rand.GetRandomItems(metroIndex, footholdIndex1001Data.triggerNumber))
		{
			float num = 2f * (float)(randomItem % 3 - 1) + combatData.nodeRootPositonX;
			float num2 = 2f * (float)(randomItem / 3 - 3) + 0.8f + combatData.nodeRootPositonY;
			((zbFootholdIndex1001)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1001.prefab", this, Vector2.op_Implicit(new Vector2(num, num2)), Vector2.up)).SetFootholdAtr(footholdIndex1001Data, footholdEffectRecord);
		}
	}
}
