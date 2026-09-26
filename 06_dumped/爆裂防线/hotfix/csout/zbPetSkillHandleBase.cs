using System;
using Proto.Types;
using UnityEngine;

public class zbPetSkillHandleBase
{
	protected WaitForEndOfFrame wait;

	[NonSerialized]
	protected float spreadAngle;

	protected AudioSource audio;

	protected zbPet pet;

	protected BattleServantSkillDetail servantSkillDetail;

	public bool actioning;

	protected float currentTime;

	public zbServantSkillCsvRecord servantSkillRecord;

	protected int targetMonsterUUID;

	protected zbHero hero;

	protected float m_refreshTimer;

	protected static readonly float m_refreshInterval = 1f;

	public int SkillId { get; protected set; }

	public virtual void Initialize(zbHero _hero, zbServantSkillCsvRecord _servantSkillRecord, zbPet _pet, BattleServantSkillDetail _servantSkillDetail)
	{
		hero = _hero;
		pet = _pet;
		servantSkillDetail = _servantSkillDetail;
		servantSkillRecord = _servantSkillRecord;
		SkillId = servantSkillRecord.id;
	}

	public virtual void Reset()
	{
		currentTime = 0f;
	}

	protected virtual void StopSelfAction()
	{
	}

	public virtual void TriggerKill()
	{
	}

	public virtual void TriggerAttacked()
	{
	}

	public bool IsActive()
	{
		return servantSkillDetail != null;
	}

	public virtual void Update()
	{
	}

	public zbPetSkillHandleBase()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		wait = new WaitForEndOfFrame();
		spreadAngle = 5f;
		base._002Ector();
	}
}
