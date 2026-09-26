using UnityEngine;

public class zbFoothold_2101 : zbFootholdBase
{
	public stFootholdIndex2101 footholdIndex2101Data = new stFootholdIndex2101();

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
		footholdIndex2101Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex2101Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex2101Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex2101Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex2101Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex2101Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex2101Data.buffId = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex2101Data.buffTime = footholdEffectRecord.param[num];
		num++;
		m_damageIntervalPre = footholdIndex2101Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex2101Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex2101Data.skillEveryTime;
			UpdateCreateSkill();
		}
	}

	private void UpdateCreateSkill()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 spawnPostion = default(Vector3);
		for (int i = 0; i < footholdIndex2101Data.skillNumber; i++)
		{
			((Vector3)(ref spawnPostion))._002Ector(Rand.Range(-3f, 3f) + combatData.nodeRootPositonX, Rand.Range(-4f, 3f) + combatData.nodeRootPositonY);
			((zbFootholdIndex2101)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex2101.prefab", this, spawnPostion, Vector2.up)).SetFootholdAtr(footholdIndex2101Data, footholdEffectRecord);
		}
	}
}
