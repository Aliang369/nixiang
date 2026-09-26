using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_1201 : zbFootholdBase
{
	public stFootholdIndex1201 footholdIndex1201Data = new stFootholdIndex1201();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private zbFootholdIndex1201[] footholdIndex1201 = new zbFootholdIndex1201[10];

	protected static readonly List<Vector2> heroVector3s;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		for (int i = 0; i < 10; i++)
		{
			footholdIndex1201[i] = (zbFootholdIndex1201)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1201.prefab", this, Vector2.op_Implicit(heroVector3s[i] + new Vector2(_combatData.nodeRootPositonX, _combatData.nodeRootPositonY)), Vector2.up);
			footholdIndex1201[i].SetFootholdAtr(footholdIndex1201Data, footholdEffectRecord);
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1201Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.slowDownTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.slowDownPrecent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1201Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		m_damageInterval = footholdIndex1201Data.skillEveryTime;
		m_damageIntervalPre = footholdIndex1201Data.skillEveryTime;
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
		foreach (zbFootholdIndex1201 randomItem in Rand.GetRandomItems(footholdIndex1201, footholdIndex1201Data.triggerNumber))
		{
			randomItem.TriggerDamage();
		}
	}

	static zbFoothold_1201()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		heroVector3s = new List<Vector2>
		{
			new Vector2(-2.88f, -5.04f),
			new Vector2(0f, -5.04f),
			new Vector2(2.88f, -5.04f),
			new Vector2(-1.44f, -2.16f),
			new Vector2(1.44f, -2.16f),
			new Vector2(-2.88f, 0.72f),
			new Vector2(0f, 0.72f),
			new Vector2(2.88f, 0.72f),
			new Vector2(-1.44f, 3.6f),
			new Vector2(1.44f, 3.6f)
		};
	}
}
