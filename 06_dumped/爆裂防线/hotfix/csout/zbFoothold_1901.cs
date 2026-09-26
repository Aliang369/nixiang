using UnityEngine;

public class zbFoothold_1901 : zbFootholdBase
{
	public stFootholdIndex1901 footholdIndex1901Data = new stFootholdIndex1901();

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
		footholdIndex1901Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1901Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1901Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex1901Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1901Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex1901Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1901Data.towPercent = footholdEffectRecord.param[num];
		num++;
		m_damageIntervalPre = footholdIndex1901Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex1901Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex1901Data.skillEveryTime;
			UpdateCreateSkill();
		}
	}

	private void UpdateCreateSkill()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 spawnPostion = default(Vector3);
		for (int i = 0; i < footholdIndex1901Data.skillNumber; i++)
		{
			((Vector3)(ref spawnPostion))._002Ector(Rand.Range(-3f, 3f) + combatData.nodeRootPositonX, Rand.Range(-4f, 3f) + combatData.nodeRootPositonY);
			((zbFootholdIndex1901)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1901.prefab", this, spawnPostion, Vector2.up)).SetFootholdAtr(footholdIndex1901Data, footholdEffectRecord);
		}
	}
}
