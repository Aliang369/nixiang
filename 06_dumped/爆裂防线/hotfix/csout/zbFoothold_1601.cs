using UnityEngine;

public class zbFoothold_1601 : zbFootholdBase
{
	public stFootholdIndex1601 footholdIndex1601Data = new stFootholdIndex1601();

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
		footholdIndex1601Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex1601Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.growTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.daizhiTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.buffId = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.hitRatePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1601Data.hitRateTime = footholdEffectRecord.param[num];
		num++;
		m_damageIntervalPre = footholdIndex1601Data.skillEveryTime;
	}

	private void UpdateCreateSkill()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < footholdIndex1601Data.skillNumber; i++)
		{
			float num = Rand.Range(-3f, 3f) + combatData.nodeRootPositonX;
			float num2 = Rand.Range(combatData.defense.GetAlertPositionY() + 1f, combatData.combatScene.top.position.y);
			((zbFootholdIndex1601)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1601.prefab", this, Vector2.op_Implicit(new Vector2(num, num2)), Vector2.up)).SetFootholdAtr(footholdIndex1601Data, footholdEffectRecord);
		}
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex1601Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex1601Data.skillEveryTime;
			UpdateCreateSkill();
		}
	}
}
