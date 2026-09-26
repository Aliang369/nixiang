using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_1701 : zbFootholdBase
{
	public stFootholdIndex1701 footholdIndex1701Data = new stFootholdIndex1701();

	private float m_damageIntervalPre;

	private zbFootholdIndex1701[] footholdIndex1701 = new zbFootholdIndex1701[3];

	private int[] metroIndex = new int[3] { 0, 1, 2 };

	protected static readonly List<float> metroPY = new List<float> { -2.4f, 0f, 2.4f };

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		((zbFootholdIndex1701_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1701_2.prefab", this, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up)).SetFootholdAtr(footholdIndex1701Data, footholdEffectRecord);
		for (int i = 0; i < 3; i++)
		{
			footholdIndex1701[i] = (zbFootholdIndex1701)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1701.prefab", this, Vector2.op_Implicit(new Vector2(metroPY[i] + combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up);
			footholdIndex1701[i].SetFootholdAtr(footholdIndex1701Data, footholdEffectRecord);
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1701Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1701Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1701Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex1701Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1701Data.upSpeed = footholdEffectRecord.param[num] / 100f;
		num++;
		m_damageIntervalPre = footholdIndex1701Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex1701Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex1701Data.skillEveryTime;
			TriggerDamage();
		}
	}

	private void TriggerDamage()
	{
		foreach (int randomItem in Rand.GetRandomItems(metroIndex, footholdIndex1701Data.skillNumber))
		{
			footholdIndex1701[randomItem].TriggerDamage();
		}
	}
}
