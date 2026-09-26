using UnityEngine;

public class zbFoothold_1101 : zbFootholdBase
{
	public stFootholdIndex1101 footholdIndex1101Data = new stFootholdIndex1101();

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		UpdateCreateSkill();
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1101Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
		footholdIndex1101Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex1101Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1101Data.slowDownTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1101Data.slowDownPrecent = footholdEffectRecord.param[num];
		num++;
		footholdIndex1101Data.durationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1101Data.cdTime = footholdEffectRecord.param[num];
		num++;
	}

	private void UpdateCreateSkill()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			float num = 1.4f * (float)(i - 1) + combatData.nodeRootPositonX;
			float num2 = Rand.Range(-4f, 3f) + combatData.nodeRootPositonY;
			((zbFootholdIndex1101)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1101.prefab", this, Vector2.op_Implicit(new Vector2(num, num2)), Vector2.up)).SetFootholdAtr(footholdIndex1101Data, footholdEffectRecord);
		}
	}
}
