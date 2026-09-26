using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbModIndex1082 : zbModBase
{
	private List<Vector2> vector2List;

	private List<Vector2> result;

	private zbHero hero;

	public override void RefreshProperty()
	{
		hero = team.GetHeroBySkillType(ESkillType.Qiang);
		base.RefreshProperty();
		int num = 0;
		team.modIndex1082.effectId = (int)modRecord.clientEff[num];
		num++;
		team.modIndex1082.fireTotleCount = (int)modRecord.clientEff[num];
		num++;
		team.modIndex1082.subCount = (int)modRecord.clientEff[num];
		num++;
		team.modIndex1082.subSkillId = (int)modRecord.clientEff[num];
		num++;
		team.modIndex1082.modIndex1082 = this;
	}

	public void OnSkillUpdate()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		GetRandomPoints(vector2List, team.modIndex1082.subCount, result);
		combatData.skillEffectPoolMgr.GetEffect(team.modIndex1082.effectId).InitData(combatData, team.modIndex1082.effectId, ((Component)hero).transform.position, "animation");
		for (int i = 0; i < team.modIndex1082.subCount; i++)
		{
			int index = Math.Min(i, 3);
			Vector2 val = result[index];
			zbSkillSystem.Instance.CreateSubSkill(team.modIndex1082.subSkillId, hero, null, hero.heroAttr.skillAttrList, Vector2.op_Implicit(val), Vector2.up);
		}
	}

	public static void GetRandomPoints(List<Vector2> source, int count, List<Vector2> result)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		result.Clear();
		int count2 = source.Count;
		if (count > count2)
		{
			count = count2;
		}
		for (int i = 0; i < count2; i++)
		{
			int index = Rand.Range(i, count2);
			Vector2 value = source[i];
			source[i] = source[index];
			source[index] = value;
		}
		for (int j = 0; j < count; j++)
		{
			result.Add(source[j]);
		}
	}

	public zbModIndex1082()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		vector2List = new List<Vector2>
		{
			new Vector2(-1.8f, 4f),
			new Vector2(1.8f, 1.5f),
			new Vector2(-1.8f, -1f),
			new Vector2(1.8f, -4f)
		};
		result = new List<Vector2>();
		base._002Ector();
	}
}
