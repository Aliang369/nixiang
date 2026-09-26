using UnityEngine;

public class zbFoothold_1401 : zbFootholdBase
{
	public stFootholdIndex1401 footholdIndex1401Data = new stFootholdIndex1401();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbFootholdIndex1401 footholdIndex1401;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		footholdIndex1401 = (zbFootholdIndex1401)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1401.prefab", this, Vector2.op_Implicit(new Vector2(_combatData.nodeRootPositonX, _combatData.nodeRootPositonY)), Vector2.up);
		footholdIndex1401.SetFootholdAtr(footholdIndex1401Data, footholdEffectRecord);
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1401Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1401Data.durationTime = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex1401Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex1401Data.skillEveryTime;
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
		footholdIndex1401.TriggerDamage();
	}
}
