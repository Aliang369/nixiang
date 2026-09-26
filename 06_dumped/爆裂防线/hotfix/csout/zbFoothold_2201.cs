using UnityEngine;

public class zbFoothold_2201 : zbFootholdBase
{
	public stFootholdIndex2201 footholdIndex2201Data = new stFootholdIndex2201();

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
		footholdIndex2201Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex2201Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex2201Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex2201Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex2201Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex2201Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		m_damageIntervalPre = footholdIndex2201Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex2201Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex2201Data.skillEveryTime;
			UpdateCreateSkill();
		}
	}

	private void UpdateCreateSkill()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Vector3 spawnPostion = default(Vector3);
		for (int i = 0; i < footholdIndex2201Data.triggerNumber; i++)
		{
			((Vector3)(ref spawnPostion))._002Ector(Rand.Range(-3f, 3f) + combatData.nodeRootPositonX, Rand.Range(-4f + combatData.nodeRootPositonY, ((Component)combatData.combatScene.top).transform.position.y - 1f));
			((zbFootholdIndex2201)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex2201.prefab", this, spawnPostion, Vector2.up)).SetFootholdAtr(footholdIndex2201Data, footholdEffectRecord);
		}
	}
}
