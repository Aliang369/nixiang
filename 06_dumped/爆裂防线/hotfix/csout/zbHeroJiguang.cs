using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbHeroJiguang : zbHeroMain
{
	protected override void OnEnable()
	{
		base.OnEnable();
		needDurationFlag = true;
	}

	public override IEnumerator ShootMonster()
	{
		needDurationProgress = GetAttrValueByType(EAttrSkillType.Duration);
		needDurationProgressMax = needDurationProgress;
		needDurationProgressMaxTemp = 0f;
		((MonoBehaviour)this).StartCoroutine(ShootClosestMonster());
		yield return null;
	}

	public override IEnumerator ShootClosestMonster()
	{
		zbMonster monsterById;
		while (true)
		{
			zbMonster closestMonster = combatData.monsters.GetClosestMonster(GetAttrValueByType(EAttrSkillType.AlertRange));
			if ((Object)(object)closestMonster == (Object)null)
			{
				yield return null;
				continue;
			}
			Vector3 val = ((Component)anim.skeletonAnimation).transform.InverseTransformPoint(closestMonster.GetCurPosition());
			if ((Object)(object)riseEffect != (Object)null)
			{
				riseEffect.AnimationState.SetAnimation(0, skillRecord.riseEffectId, true);
			}
			int targetMonsterId = closestMonster.GetUUID();
			anim.skeletonAnimation.AnimationState.SetAnimation(0, "attack", true);
			anim.skeletonAnimation.AnimationState.SetAnimation(1, "aim", true);
			if (boneShoot != null)
			{
				SkeletonExtensions.SetLocalPosition(boneShoot, val);
			}
			yield return null;
			monsterById = combatData.monsters.GetMonsterById(targetMonsterId);
			if (!((Object)(object)monsterById == (Object)null))
			{
				break;
			}
			yield return null;
		}
		FireToTargetPosition(monsterById.GetUUID(), monsterById.GetCurPosition());
		int num = (int)GetAttrValueByType(EAttrSkillType.Extra);
		for (int i = 0; i < num; i++)
		{
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if (Object.op_Implicit((Object)(object)randomMonster))
			{
				FireToTargetPosition(randomMonster.GetUUID(), randomMonster.GetCurPosition());
			}
		}
		isCDFlag = false;
		nextFireTime = 0f;
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
