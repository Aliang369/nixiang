using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbHeroShexian : zbHeroMain
{
	protected override void OnEnable()
	{
		base.OnEnable();
		needDurationFlag = true;
	}

	public override IEnumerator ShootMonster()
	{
		needDurationProgress = GetAttrValueByType(EAttrSkillType.Duration);
		if (team.ExtraProperties[ETeamExtra.ShexianMod2112] > 0f)
		{
			needDurationProgress = GetAttrValueByType(EAttrSkillType.Duration) * 2f;
		}
		needDurationProgressMax = needDurationProgress;
		needDurationProgressMaxTemp = 0f;
		((MonoBehaviour)this).StartCoroutine(ShootClosestMonster());
		int attrExtra = (int)GetAttrValueByType(EAttrSkillType.Extra);
		int i = 0;
		while (i < attrExtra)
		{
			yield return (object)new WaitForSeconds(extraBulletInterval);
			((MonoBehaviour)this).StartCoroutine(ShootRandomMonster());
			int num = i + 1;
			i = num;
		}
	}

	public override IEnumerator ShootClosestMonster()
	{
		zbMonster closestMonster;
		while (true)
		{
			closestMonster = combatData.monsters.GetClosestMonster(GetAttrValueByType(EAttrSkillType.AlertRange));
			if (!((Object)(object)closestMonster == (Object)null))
			{
				break;
			}
			yield return null;
		}
		if ((Object)(object)riseEffect != (Object)null)
		{
			riseEffect.AnimationState.SetAnimation(0, skillRecord.riseEffectId, true);
		}
		anim.skeletonAnimation.AnimationState.SetAnimation(0, "attack", true);
		int uuid = closestMonster.GetUUID();
		Vector3 vector = closestMonster.GetCurPosition();
		AdjustHeroRotation(vector);
		yield return null;
		FireToTargetPosition(uuid, vector);
		isCDFlag = false;
		nextFireTime = 0f;
	}

	public override IEnumerator ShootRandomMonster()
	{
		zbMonster randomMonster = combatData.monsters.GetRandomMonster();
		if (Object.op_Implicit((Object)(object)randomMonster))
		{
			FireToTargetPosition(randomMonster.GetUUID(), randomMonster.GetCurPosition());
		}
		yield return null;
	}

	public override void AdjustHeroRotation(Vector3 targetPosition)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		anim.skeletonAnimation.AnimationState.SetAnimation(1, "aim", true);
		Vector2 val = Vector2.op_Implicit(zbUtil.GetTargetNormalized(GetGunpointPosition(), targetPosition));
		float num = Vector2.Angle(Vector2.up, val);
		if (targetPosition.x < 0f && num > 15f)
		{
			if (((SkeletonRenderer)anim.skeletonAnimation).skeleton.Data.FindAnimation("turn") != null)
			{
				anim.skeletonAnimation.AnimationState.SetAnimation(2, "turn", true);
			}
		}
		else
		{
			anim.skeletonAnimation.AnimationState.ClearTrack(2);
		}
	}

	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 gunpointPosition = GetGunpointPosition();
		zbSkillSystem.Instance.CreateSkill(this, gunpointPosition, Vector2.up, targetMonsterId);
	}
}
