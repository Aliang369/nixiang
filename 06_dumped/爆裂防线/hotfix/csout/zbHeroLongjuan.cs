using System.Collections;
using Proto.Enums;
using UnityEngine;

public class zbHeroLongjuan : zbHeroMain
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
		((MonoBehaviour)this).StartCoroutine(base.ShootMonster());
		yield return null;
	}

	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		zbSkillSystem.Instance.CreateSkill(this, targetPosition, Vector2.up, targetMonsterId);
		zbAudioMgr.Instance.PlayAudio("audio/combat/bingbaofashengqi_fire.ogg");
	}
}
