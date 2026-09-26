using System;
using Proto.Types;
using UnityEngine;

public class zbMechaSkillHandleBase
{
	protected WaitForEndOfFrame wait;

	[NonSerialized]
	protected float spreadAngle;

	protected AudioSource audio;

	protected zbMechaBase mecha;

	protected BattleMechaSkillDetail mechaSkillDetail;

	public bool actioning;

	protected float currentTime;

	public zbMechaSkillCsvRecord mechaSkillRecord;

	protected int targetMonsterUUID;

	protected bool isNear;

	protected string skillName1;

	protected string skillName2;

	protected float skillName2Time;

	public int SkillId { get; protected set; }

	public float damageRate { get; set; }

	public zbMechaSkillHandleBase(int _SkillId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		wait = new WaitForEndOfFrame();
		spreadAngle = 5f;
		base._002Ector();
		mechaSkillRecord = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(_SkillId);
		SkillId = _SkillId;
	}

	public virtual void Initialize(zbMechaBase _mecha, BattleMechaSkillDetail _mechaSkillDetail)
	{
		mecha = _mecha;
		mechaSkillDetail = _mechaSkillDetail;
		if (mechaSkillDetail != null)
		{
			damageRate = mechaSkillDetail.DamageRate;
		}
	}

	public virtual void Reset()
	{
		currentTime = 0f;
		actioning = false;
	}

	protected virtual void StopSelfAction()
	{
	}

	public bool IsActive()
	{
		return mechaSkillDetail != null;
	}

	public virtual void Update()
	{
	}
}
