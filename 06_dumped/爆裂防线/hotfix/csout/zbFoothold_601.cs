using System.Collections.Generic;
using UnityEngine;

public class zbFoothold_601 : zbFootholdBase
{
	public stFootholdIndex601 footholdIndex601Data = new stFootholdIndex601();

	private float m_damageIntervalPre;

	private zbFootholdIndex601[] footholdIndex601 = new zbFootholdIndex601[2];

	private int[] metroIndex = new int[4] { 0, 1, 2, 3 };

	protected static readonly List<float> metroPY = new List<float> { -4.3f, -1.4f, 1.5f, 4.4f };

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		for (int i = 0; i < 2; i++)
		{
			footholdIndex601[i] = (zbFootholdIndex601)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex601.prefab", this, Vector2.op_Implicit(new Vector2(300f, 0f)), Vector2.up);
			footholdIndex601[i].SetFootholdAtr(footholdIndex601Data, footholdEffectRecord);
			if (i == 0)
			{
				footholdIndex601[i].m_moveUp = Vector3.left;
				((Component)footholdIndex601[i]).transform.localScale = Vector3.one;
			}
			else
			{
				footholdIndex601[i].m_moveUp = Vector3.right;
				((Component)footholdIndex601[i]).transform.localScale = Vector3.one + 2f * Vector3.left;
			}
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex601Data.skillEveryTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex601Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex601Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex601Data.knockbackPercent = footholdEffectRecord.param[num];
		num++;
		m_damageIntervalPre = footholdIndex601Data.skillEveryTime;
	}

	protected override void Update()
	{
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= footholdIndex601Data.skillEveryTime)
		{
			m_damageIntervalPre -= footholdIndex601Data.skillEveryTime;
			TriggerDamage();
		}
	}

	private void TriggerDamage()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (int randomItem in Rand.GetRandomItems(metroIndex, 2))
		{
			float num2 = 0f;
			num2 = ((num != 0) ? (combatData.nodeRootPositonX - 5f - 3.6f) : (combatData.nodeRootPositonX + 5f + 3.6f));
			((Component)footholdIndex601[num]).transform.position = new Vector3(num2, metroPY[randomItem], ((Component)footholdIndex601[num]).transform.position.z);
			footholdIndex601[num].TriggerDamage();
			num++;
		}
	}
}
