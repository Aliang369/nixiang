using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_1501 : zbFootholdBase
{
	public stFootholdIndex1501 footholdIndex1501Data = new stFootholdIndex1501();

	private zbFootholdIndex1501[] footholdIndex1501 = new zbFootholdIndex1501[3];

	private float m_stoneEveryTimePre;

	private float m_carEveryTimePre;

	private int[] metroIndex = new int[3] { 0, 1, 2 };

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1501Data.stoneEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.stoneNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex1501Data.stoneDamagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.stoneKnockbackPercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.carEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.carNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.carDamageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.carDamagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.carKnockbackPercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1501Data.moveSpeed = footholdEffectRecord.param[num];
		num++;
		m_carEveryTimePre = footholdIndex1501Data.carEveryTime;
		m_stoneEveryTimePre = footholdIndex1501Data.stoneEveryTime;
	}

	protected override void Update()
	{
		m_carEveryTimePre += Time.deltaTime;
		if (m_carEveryTimePre >= footholdIndex1501Data.carEveryTime)
		{
			m_carEveryTimePre = 0f;
			TriggerDamageCar();
		}
		m_stoneEveryTimePre += Time.deltaTime;
		if (m_stoneEveryTimePre >= footholdIndex1501Data.stoneEveryTime)
		{
			m_stoneEveryTimePre = 0f;
			TriggerDamageStone();
		}
	}

	private void TriggerDamageStone()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 spawnPostion = default(Vector3);
		for (int i = 0; i < footholdIndex1501Data.stoneNumber; i++)
		{
			((Vector3)(ref spawnPostion))._002Ector(Rand.Range(-3f, 3f) + combatData.nodeRootPositonX, Rand.Range(-4f, 3f) + combatData.nodeRootPositonY);
			((zbFootholdIndex1501_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1501_2.prefab", this, spawnPostion, Vector2.up)).SetFootholdAtr(footholdIndex1501Data, footholdEffectRecord);
		}
	}

	private void TriggerDamageCar()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		List<int> randomItems = Rand.GetRandomItems(metroIndex, footholdIndex1501Data.carNumber);
		for (int i = 0; i < footholdIndex1501Data.carNumber; i++)
		{
			float num = 2.1f * (float)(randomItems[i] - 1) + combatData.nodeRootPositonX;
			footholdIndex1501[i] = (zbFootholdIndex1501)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1501.prefab", this, Vector2.op_Implicit(new Vector2(num, 0f)), Vector2.up);
			footholdIndex1501[i].SetFootholdAtr(footholdIndex1501Data, footholdEffectRecord);
			footholdIndex1501[i].TriggerDamage();
		}
	}
}
