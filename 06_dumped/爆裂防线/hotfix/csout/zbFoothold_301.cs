using UnityEngine;

public class zbFoothold_301 : zbFootholdBase
{
	public stFootholdIndex301 footholdIndex301Data = new stFootholdIndex301();

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
		footholdIndex301Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex301Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.scalePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.massPercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.slowDownPrecent = footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.slowDownTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex301Data.massLimit = footholdEffectRecord.param[num];
		num++;
		m_damageInterval = footholdIndex301Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex301Data.skillEveryTime;
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
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 spawnPostion = default(Vector3);
		for (int i = 0; i < footholdIndex301Data.skillNumber; i++)
		{
			((Vector3)(ref spawnPostion))._002Ector(Rand.Range(-3f, 3f) + combatData.nodeRootPositonX, Rand.Range(-4f, 3f) + combatData.nodeRootPositonY);
			((zbFootholdIndex301)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex301.prefab", this, spawnPostion, Vector2.up)).SetFootholdAtr(footholdIndex301Data, footholdEffectRecord);
		}
	}
}
