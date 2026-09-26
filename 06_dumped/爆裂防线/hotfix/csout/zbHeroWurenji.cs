using System.Collections;
using Proto.Enums;
using UnityEngine;

public class zbHeroWurenji : zbHeroMain
{
	protected override void OnEnable()
	{
		base.OnEnable();
		needDurationFlag = true;
	}

	public override IEnumerator ShootMonster()
	{
		((MonoBehaviour)this).StartCoroutine(base.ShootMonster());
		needDurationProgress = GetAttrValueByType(EAttrSkillType.Duration);
		needDurationProgressMax = needDurationProgress;
		needDurationProgressMaxTemp = 0f;
		yield return null;
	}

	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		anim.skeletonAnimation.AnimationState.SetAnimation(0, "attack", true);
		zbSkillSystem.Instance.CreateSkill(this, ((Component)this).transform.position, Vector2.up, targetMonsterId);
		zbAudioMgr.Instance.PlayAudio("audio/combat/bingbaofashengqi_fire.ogg");
	}
}
