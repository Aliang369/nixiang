using UnityEngine;

public class zbFoothold_501 : zbFootholdBase
{
	public stFootholdIndex501 footholdIndex501Data = new stFootholdIndex501();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbFootholdIndex501[] footholdIndex501 = new zbFootholdIndex501[30];

	private int[] metroIndex = new int[6] { 0, 1, 2, 3, 4, 5 };

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		for (int i = 0; i < 6; i++)
		{
			float num = 0f;
			float num2 = -4.5f + 2f * (float)i + combatData.nodeRootPositonY;
			for (int j = 0; j < 5; j++)
			{
				num = 1.4f * (float)(j - 2) + combatData.nodeRootPositonX;
				footholdIndex501[i + j * 6] = (zbFootholdIndex501)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex501.prefab", this, new Vector3(num, num2), Vector2.up);
				footholdIndex501[i + j * 6].SetFootholdAtr(footholdIndex501Data, footholdEffectRecord);
			}
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex501Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex501Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex501Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex501Data.dazeTime = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex501Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex501Data.skillEveryTime;
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
		foreach (int randomItem in Rand.GetRandomItems(metroIndex, footholdIndex501Data.triggerNumber))
		{
			for (int i = 0; i < 5; i++)
			{
				footholdIndex501[5 * randomItem + i].TriggerDamage();
			}
		}
	}
}
