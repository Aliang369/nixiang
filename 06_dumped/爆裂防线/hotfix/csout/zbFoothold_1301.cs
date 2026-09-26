using UnityEngine;

public class zbFoothold_1301 : zbFootholdBase
{
	public stFootholdIndex1301 footholdIndex1301Data = new stFootholdIndex1301();

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, int _FootholdId)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		base.InitData(_combatData, _team, _FootholdId);
		RefreshParam();
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			float num2 = -4.5f + 1.3f * (float)i + combatData.nodeRootPositonY;
			zbFootholdIndex1301 obj = (zbFootholdIndex1301)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1301.prefab", this, new Vector3(-3f + combatData.nodeRootPositonX, num2), Vector2.up);
			obj.SetFootholdAtr(footholdIndex1301Data, footholdEffectRecord);
			obj.SetScaleFlip(scaleFlip: false);
			zbFootholdIndex1301 obj2 = (zbFootholdIndex1301)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex1301.prefab", this, new Vector3(3f + combatData.nodeRootPositonX, num2), Vector2.up);
			obj2.SetFootholdAtr(footholdIndex1301Data, footholdEffectRecord);
			obj2.SetScaleFlip(scaleFlip: true);
		}
	}

	public override void RefreshParam()
	{
		base.RefreshParam();
		int num = 0;
		footholdIndex1301Data.triggerNumber = (int)footholdEffectRecord.param[num];
		num++;
		footholdIndex1301Data.eatTime = footholdEffectRecord.param[num];
		num++;
		footholdIndex1301Data.massLimit = (int)footholdEffectRecord.param[num];
		num++;
	}
}
