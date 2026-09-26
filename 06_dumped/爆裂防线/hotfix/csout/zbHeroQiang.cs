using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbHeroQiang : zbHero
{
	private bool initAmmo;

	[NonSerialized]
	public int maxAmmo = 1;

	public zbSafeInt currentAmmo = new zbSafeInt(30, "zbHeroQiang.currentAmmo");

	[NonSerialized]
	public float reloadTime = 2f;

	[NonSerialized]
	public bool isReloading;

	[NonSerialized]
	public float reloadedTime;

	[NonSerialized]
	public float progress = 100f;

	[NonSerialized]
	public int hitNumber;

	[NonSerialized]
	public float limitlessTime;

	private int targetMonsterId = zbMonster.sInvalidId;

	public int reloadNum;

	protected override void Awake()
	{
		base.Awake();
		progress = 100f;
		nextFireTime = 0f;
	}

	public override void OnAfterModCreated()
	{
		maxAmmo = (int)GetAttrValueByType(EAttrSkillType.Magazine);
		if (!initAmmo)
		{
			SetCurrentAmmo(maxAmmo);
			initAmmo = true;
		}
	}

	public override void Restore(List<stRecordHero> heroList, stRecordHeroQiang heroQiang = null)
	{
		base.Restore(heroList);
		maxAmmo = heroQiang.maxAmmo;
		SetCurrentAmmo(Math.Max(heroQiang.currentAmmo, 1));
		hitNumber = heroQiang.hitNumber;
		limitlessTime = heroQiang.limitlessTime;
	}

	public void SetCurrentAmmo(int value)
	{
		currentAmmo.Value = Math.Min(Math.Max(value, 0), 30);
	}

	public override void OnSkillUpdate()
	{
		if (team.modIndex1081.Count > 0)
		{
			team.Trigger1081();
		}
		limitlessTime = Math.Max(0f, limitlessTime - Time.deltaTime);
		if (limitlessTime > 0f)
		{
			LimitlessSkillUpdate();
		}
		else
		{
			ExecSkillUpdate();
		}
	}

	public void ExecSkillUpdate()
	{
		if (zbCombatSystem.Instance.m_isCelebrating)
		{
			return;
		}
		if (team.ExtraProperties[ETeamExtra.GemMod2011_1] > 0f && (float)hitNumber >= team.ExtraProperties[ETeamExtra.GemMod2011_1])
		{
			limitlessTime = team.ExtraProperties[ETeamExtra.GemMod2011_2];
			hitNumber = 0;
			return;
		}
		if (isReloading)
		{
			reloadedTime += Time.deltaTime;
			progress = reloadedTime / reloadTime * 100f;
			return;
		}
		if (!combatData.monsters.HasMonsterInAlertRange(GetAttrValueByType(EAttrSkillType.AlertRange)))
		{
			nextFireTime = 0f;
			return;
		}
		nextFireTime = Time.deltaTime + nextFireTime;
		if (nextFireTime >= 1f / GetAttrValueByType(EAttrSkillType.FireRate))
		{
			hitNumber++;
			((MonoBehaviour)this).StartCoroutine(ShootMonster());
			SetCurrentAmmo((int)currentAmmo - 1);
			nextFireTime = 0f;
		}
	}

	public void LimitlessSkillUpdate()
	{
		nextFireTime = Time.deltaTime + nextFireTime;
		if (nextFireTime >= 1f / GetAttrValueByType(EAttrSkillType.FireRate) / 2f && !isCDFlag)
		{
			isCDFlag = true;
			((MonoBehaviour)this).StartCoroutine(ShootMonster());
		}
	}

	public override IEnumerator ShootMonster()
	{
		((MonoBehaviour)this).StartCoroutine(ShootClosestMonster());
		yield return null;
	}

	public override IEnumerator ShootClosestMonster()
	{
		zbMonster zbMonster2;
		while (true)
		{
			zbMonster2 = combatData.monsters.GetMonsterById(targetMonsterId);
			if ((Object)(object)zbMonster2 == (Object)null || !zbMonster2.isCanSelected())
			{
				zbMonster2 = combatData.monsters.GetClosestMonster(GetAttrValueByType(EAttrSkillType.AlertRange));
				if ((Object)(object)zbMonster2 != (Object)null)
				{
					targetMonsterId = zbMonster2.GetUUID();
				}
			}
			if ((Object)(object)zbMonster2 != (Object)null)
			{
				break;
			}
			yield return null;
		}
		Vector3 targetPosition = zbMonster2.GetCurPosition();
		AdjustHeroRotation(targetPosition);
		isCDFlag = false;
		nextFireTime = 0f;
		yield return null;
		yield return null;
		if (!((Object)(object)combatData.monsters.GetMonsterById(targetMonsterId) == (Object)null))
		{
			FireToPosition(targetPosition);
			if ((int)currentAmmo <= 0)
			{
				((MonoBehaviour)this).StartCoroutine(Reload());
			}
		}
	}

	public void FireToPosition(Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 gunpointPosition = GetGunpointPosition();
		Vector3 val = targetPosition - gunpointPosition;
		Vector2 directionToTarget = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
		int num = (int)GetAttrValueByType(EAttrSkillType.Repeater);
		for (int i = 0; i < num + 1; i++)
		{
			int number = i;
			((MonoBehaviour)this).StartCoroutine(ShootRepeater(directionToTarget, gunpointPosition, number));
		}
	}

	public IEnumerator ShootRepeater(Vector2 directionToTarget, Vector3 offsetPosition, int number)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		yield return (object)new WaitForSeconds((float)number * 0.1f);
		zbAudioMgr.Instance.PlayAudio("audio/combat/gun.ogg");
		int num = (int)GetAttrValueByType(EAttrSkillType.Volley);
		for (int i = 0; i < num + 1; i++)
		{
			int num2 = (int)Math.Ceiling((float)i / 2f);
			float num3 = spreadAngle * (float)num2;
			if (i % 2 == 1)
			{
				num3 = (0f - spreadAngle) * (float)num2;
			}
			Vector2 direction = Vector2.op_Implicit(Quaternion.Euler(0f, 0f, num3) * Vector2.op_Implicit(directionToTarget));
			zbSkillSystem.Instance.CreateSkill(this, offsetPosition, Vector2.up, targetMonsterId).SetDirection(direction);
		}
	}

	private IEnumerator Reload()
	{
		if (!isReloading)
		{
			reloadTime = GetAttrValueByType(EAttrSkillType.ReloadTime);
			if (zbUtil.Roll0to1(team.ExtraProperties[ETeamExtra.GemMod2012_1]))
			{
				reloadTime *= 1f + team.ExtraProperties[ETeamExtra.GemMod2012_2];
			}
			AddReload();
			isReloading = true;
			anim.skeletonAnimation.AnimationState.SetAnimation(0, zbCombatDefine.ANIMATION_SEQUENCE_RELOAD, true);
			anim.skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
			yield return (object)new WaitForSeconds(reloadTime);
			anim.skeletonAnimation.AnimationState.SetAnimation(0, zbCombatDefine.ANIMATION_SEQUENCE_IDLE, true);
			SetCurrentAmmo(maxAmmo);
			isReloading = false;
			reloadedTime = 0f;
			progress = 100f;
			if (team.ExtraProperties[ETeamExtra.GemMod2013] > 0f)
			{
				limitlessTime = team.ExtraProperties[ETeamExtra.GemMod2013];
			}
		}
	}

	public void SetTargetMonsterId(int _targetMonsterId)
	{
		targetMonsterId = _targetMonsterId;
	}

	public void AddReload()
	{
		reloadNum++;
	}
}
