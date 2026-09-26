using UnityEngine;

public class zbFoothold_901 : zbFootholdBase
{
	public stFootholdIndex901 footholdIndex901Data = new stFootholdIndex901();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex901Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex901Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex901Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex901Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex901Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex901Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex901Data.growTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex901Data.daizhiTime = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex901Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex901Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateCreateSkill();
		}
	}

	private void UpdateCreateSkill()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < footholdIndex901Data.skillNumber; i++)
		{
			float num = Rand.Range(-3f, 3f) + combatData.nodeRootPositonX;
			float num2 = Rand.Range(combatData.defense.GetAlertPositionY() + 1f, combatData.combatScene.top.position.y);
			((zbFootholdIndex901)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex901.prefab", this, Vector2.op_Implicit(new Vector2(num, num2)), Vector2.up)).SetFootholdAtr(footholdIndex901Data, footholdEffectRecord);
		}
	}
}
