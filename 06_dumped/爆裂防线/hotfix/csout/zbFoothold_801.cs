using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_801 : zbFootholdBase
{
	public stFootholdIndex801 footholdIndex801Data = new stFootholdIndex801();

	private float m_damageIntervalPre;

	private zbFootholdIndex801[] footholdIndex801 = new zbFootholdIndex801[4];

	private int[] metroIndex = new int[4] { 0, 1, 2, 3 };

	protected static readonly List<float> metroPY = new List<float> { -4.3f, -1.4f, 1.5f, 4.4f };

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		for (int i = 0; i < 4; i++)
		{
			footholdIndex801[i] = (zbFootholdIndex801)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex801.prefab", this, Vector2.op_Implicit(new Vector2(_combatData.nodeRootPositonX, metroPY[i])), Vector2.up);
			footholdIndex801[i].SetFootholdAtr(footholdIndex801Data, footholdEffectRecord);
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex801Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex801Data.skillNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex801Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex801Data.massLimit = footholdEffectRecord.param[num];
		num++;
		m_damageIntervalPre = footholdIndex801Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex801Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex801Data.skillEveryTime;
			TriggerDamage();
		}
	}

	private void TriggerDamage()
	{
		foreach (int randomItem in Rand.GetRandomItems(metroIndex, footholdIndex801Data.skillNumber))
		{
			footholdIndex801[randomItem].TriggerDamage();
		}
	}
}
