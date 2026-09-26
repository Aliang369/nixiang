using UnityEngine;

public class zbFoothold_401 : zbFootholdBase
{
	public stFootholdIndex401 footholdIndex401Data = new stFootholdIndex401();

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		for (int i = 0; i < 5; i++)
		{
			float num = 1.4f * (float)(i - 2) + combatData.nodeRootPositonX;
			((zbFootholdIndex401)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex401.prefab", this, Vector2.op_Implicit(new Vector2(num, 0f)), Vector2.up)).SetFootholdAtr(footholdIndex401Data, footholdEffectRecord);
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex401Data.damagePercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex401Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex401Data.knockbackPercent = footholdEffectRecord.param[num];
		num++;
		footholdIndex401Data.moveSpeed = footholdEffectRecord.param[num];
		num++;
		footholdIndex401Data.damageRange = footholdEffectRecord.param[num] / 100f;
		num++;
	}
}
