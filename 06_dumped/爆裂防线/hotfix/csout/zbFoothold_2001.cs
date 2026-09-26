using UnityEngine;

public class zbFoothold_2001 : zbFootholdBase
{
	public stFootholdIndex2001 footholdIndex2001Data = new stFootholdIndex2001();

	private zbFootholdIndex2001 footholdIndex2001;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		footholdIndex2001 = (zbFootholdIndex2001)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex2001.prefab", this, Vector2.op_Implicit(new Vector2(_combatData.nodeRootPositonX + 3.77f, _combatData.nodeRootPositonY + 0.4f)), Vector2.up);
		footholdIndex2001.SetFootholdAtr(footholdIndex2001Data, footholdEffectRecord);
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex2001Data.rotationTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex2001Data.damageInterval = footholdEffectRecord.param[num];
		num++;
		footholdIndex2001Data.slowDownPrecent = footholdEffectRecord.param[num];
		num++;
		footholdIndex2001Data.slowDownTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex2001Data.knockbackPercent = footholdEffectRecord.param[num];
		num++;
	}
}
