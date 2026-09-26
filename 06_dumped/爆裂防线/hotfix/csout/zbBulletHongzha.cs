using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbBulletHongzha : zbSkillBase
{
	private GameObject zhadanGameObject;

	private SkeletonAnimation zhadanAnimation;

	private float hitTime = 1f;

	private Vector3 targetP;

	private bool canMove = true;

	private float hitTimeEvery;

	private float zhadanSpeed = 20f;

	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zhadanGameObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonZhadan");
		zhadanAnimation = zhadanGameObject.GetComponent<SkeletonAnimation>();
		zhadanGameObject.SetActive(false);
		targetP = ((Component)this).transform.position;
		hitTimeEvery = 0f;
		hitTime = 14f / zhadanSpeed;
		float num = (float)Math.Pow(GetAttrValueByType(EAttrSkillType.ExplosionRange), 0.30000001192092896);
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		((MonoBehaviour)this).StartCoroutine(EnableColliderAfterDelay(hitTime));
	}

	protected override void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (canMove)
		{
			zhadanGameObject.transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(zhadanGameObject.transform.position), Vector2.op_Implicit(targetP), zhadanSpeed * Time.deltaTime));
			hitTimeEvery += Time.deltaTime;
			if (hitTimeEvery >= hitTime)
			{
				TrackEntry val = PlayBulletAnim("3", loop: false);
				ReturnToPool(val.AnimationEnd);
				zbAudioMgr.Instance.PlayAudio("audio/combat/kongtouhongzha_hit.ogg");
				canMove = false;
				float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
				((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
				boundsCircle2D.center.x = ((Component)this).transform.position.x;
				boundsCircle2D.center.y = ((Component)this).transform.position.y;
				boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
				UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.ExplosionDamage);
			}
		}
	}

	private IEnumerator EnableColliderAfterDelay(float delay)
	{
		yield return null;
		PlayBulletAnim("2", loop: true);
		canMove = true;
		zhadanGameObject.transform.position = new Vector3(((Component)this).transform.position.x, targetP.y + 14f, ((Component)this).transform.position.z);
		zhadanGameObject.SetActive(true);
	}
}
