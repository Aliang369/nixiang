using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbMechaBase : zbAttackerBase
{
	public readonly List<zbMechaSkillHandleBase> skills = new List<zbMechaSkillHandleBase>();

	[NonSerialized]
	public GameObject goRiseEffect_1;

	[NonSerialized]
	public GameObject goRiseEffect_2;

	[NonSerialized]
	public GameObject goRiseEffect_3;

	[NonSerialized]
	public GameObject goRiseEffect_4;

	[NonSerialized]
	public GameObject goRiseEffect_5;

	[NonSerialized]
	public zbMechaCsvRecord mechaRecord;

	public HashSet<int> ignoreList = new HashSet<int>();

	private float[] sumSkillRecord = new float[5];

	public int MechaId { get; protected set; }

	public bool mechaActiving { get; set; }

	public float Damage { get; set; }

	public virtual void InitData(zbCombatData _combatData, zbCombatTeam _team)
	{
		combatData = _combatData;
		team = _team;
		SetFightPeopleType(EFightPeopleType.Mecha);
		MechaId = team.battleTeamInfo.MechaInfo.MechaId;
		heroAttr = new zbHeroAttr();
		heroAttr.Init(this);
		heroAttr.RefreshMechaAttr();
		mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(MechaId);
		mechaAvatarRecord = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(mechaRecord.avatarId);
		foreach (BattleMechaSkillDetail skill in team.battleTeamInfo.MechaInfo.SkillList)
		{
			zbMechaSkillHandleBase zbMechaSkillHandleBase2 = combatData.mechaRegister.CreateSkill(skill.SkillId);
			zbMechaSkillHandleBase2.Initialize(this, skill);
			skills.Add(zbMechaSkillHandleBase2);
		}
		anim = new zbFightAnimation();
		anim.Init(this);
		goRiseEffect_1 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_1")).gameObject;
		goRiseEffect_2 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_2")).gameObject;
		goRiseEffect_3 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_3")).gameObject;
		goRiseEffect_4 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_4")).gameObject;
		goRiseEffect_5 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_5")).gameObject;
		Init();
	}

	public virtual void Init()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = new Vector3(-20f, -20f, 0f);
		m_sortType = ESortLayerType.Hero;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		((Component)this).transform.localScale = new Vector3(mechaRecord.bodyScale, mechaRecord.bodyScale, 1f);
	}

	protected BattleMechaSkillDetail GetMechaSkillDetail(int skillId)
	{
		foreach (BattleMechaSkillDetail skill in team.battleTeamInfo.MechaInfo.SkillList)
		{
			if (skill.SkillId == skillId)
			{
				return skill;
			}
		}
		return null;
	}

	public zbMechaSkillHandleBase GetSkillByIndex(int index)
	{
		if (index > 5)
		{
			return null;
		}
		if (skills.Count < index)
		{
			return null;
		}
		return skills[index - 1];
	}

	public void TriggerMecha()
	{
		mechaActiving = true;
		Damage = GetAttrValueByType(EAttrSkillType.Damage);
		ActionBegin();
	}

	public virtual void ActionBegin()
	{
	}

	public virtual IEnumerator Action()
	{
		yield return ActionEnter();
		if (IsZuobi(0))
		{
			yield break;
		}
		yield return ActionSkill1();
		if (IsZuobi(1))
		{
			yield break;
		}
		zbMechaSkillHandleBase skillByIndex = GetSkillByIndex(2);
		if (skillByIndex != null && skillByIndex.IsActive())
		{
			yield return ActionSkill2();
		}
		if (IsZuobi(2))
		{
			yield break;
		}
		skillByIndex = GetSkillByIndex(3);
		if (skillByIndex != null && skillByIndex.IsActive())
		{
			yield return ActionSkill3();
		}
		if (IsZuobi(3))
		{
			yield break;
		}
		skillByIndex = GetSkillByIndex(4);
		if (skillByIndex != null && skillByIndex.IsActive())
		{
			yield return ActionSkill4();
		}
		if (!IsZuobi(4))
		{
			skillByIndex = GetSkillByIndex(5);
			if (skillByIndex != null && skillByIndex.IsActive())
			{
				yield return ActionSkill5();
			}
			yield return ActionExit();
		}
	}

	public virtual IEnumerator ActionEnter()
	{
		yield return null;
	}

	public virtual IEnumerator ActionSkill1()
	{
		yield return null;
	}

	public virtual IEnumerator ActionSkill2()
	{
		yield return null;
	}

	public virtual IEnumerator ActionSkill3()
	{
		yield return null;
	}

	public virtual IEnumerator ActionSkill4()
	{
		yield return null;
	}

	public virtual IEnumerator ActionSkill5()
	{
		yield return null;
	}

	public virtual IEnumerator ActionExit()
	{
		yield return null;
	}

	protected virtual void OnTriggerEnter2D(Collider2D collider)
	{
	}

	protected virtual void OnTriggerExit2D(Collider2D collider)
	{
	}

	public virtual bool IsZuobi(int index)
	{
		GetSkillByIndex(index + 1)?.IsActive();
		return false;
	}

	public override int GetSkillType()
	{
		return 101;
	}
}
