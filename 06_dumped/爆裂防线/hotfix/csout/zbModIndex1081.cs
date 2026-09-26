using Proto.Enums;
using UnityEngine;

public class zbModIndex1081 : zbModBase
{
	private stModIndex1081 data = new stModIndex1081();

	private float cdTime;

	private zbHero hero;

	public override void RefreshProperty()
	{
		hero = team.GetHeroBySkillType(ESkillType.Qiang);
		base.RefreshProperty();
		data.alertRange = modRecord.clientEff[0];
		data.subSkillId = (int)modRecord.clientEff[1];
		data.cd = (int)modRecord.clientEff[2];
		data.modIndex1081 = this;
		team.modIndex1081.Add(data);
		cdTime = data.cd;
	}

	public void OnSkillUpdate()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (cdTime > (float)data.cd && combatData.monsters.HasMonsterInAlertRange(data.alertRange / 100f))
		{
			zbMonster closestMonster = combatData.monsters.GetClosestMonster(12f);
			if ((Object)(object)closestMonster != (Object)null && (Object)(object)hero != (Object)null)
			{
				zbSkillSystem.Instance.CreateSubSkill(data.subSkillId, hero, null, hero.heroAttr.skillAttrList, ((Component)hero).transform.position, Vector2.up, closestMonster.GetUUID());
				cdTime = 0f;
			}
		}
		cdTime += Time.deltaTime;
	}
}
