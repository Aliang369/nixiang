using System;
using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSub40601 : zbSkillBase
{
	private GameObject feijiGameObject;

	private GameObject zhadanGameObject;

	private SkeletonAnimation boomAnimation;

	private float feijiSpeed = 12f;

	private float zhadanSpeed = 20f;

	private float hitTime = 1f;

	private bool canMove = true;

	private zbBoundsCircle2D boundsCircle2D;

	private float damageRange = 1f;

	private Vector3 targetP;

	protected override void OnInited()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		canMove = false;
		feijiGameObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonFeiji");
		feijiGameObject.SetActive(false);
		zhadanGameObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonZhadan");
		zhadanGameObject.SetActive(false);
		zbAudioMgr.Instance.PlayAudio("audio/combat/kongtouhongzha_fire.ogg");
		boomAnimation = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonBoom").GetComponent<SkeletonAnimation>();
		((Component)boomAnimation).gameObject.SetActive(false);
		hitTime = 14f / zhadanSpeed;
		targetP = ((Component)this).transform.position;
		damageRange = GetAttrValueByType(EAttrSkillType.ExplosionRange);
		((MonoBehaviour)this).StartCoroutine(EnableColliderAfterDelay(hitTime));
	}

	protected override void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		feijiGameObject.transform.Translate(Vector3.up * feijiSpeed * Time.deltaTime, (Space)0);
		if (canMove)
		{
			zhadanGameObject.transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(zhadanGameObject.transform.position), Vector2.op_Implicit(targetP), zhadanSpeed * Time.deltaTime));
		}
	}

	private IEnumerator EnableColliderAfterDelay(float delay)
	{
		float num = (float)Math.Pow(damageRange, 0.30000001192092896);
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		yield return null;
		feijiGameObject.transform.position = new Vector3(feijiGameObject.transform.position.x, -9f, feijiGameObject.transform.position.z);
		feijiGameObject.SetActive(true);
		zhadanGameObject.transform.position = new Vector3(((Component)this).transform.position.x, targetP.y + 14f, targetP.z);
		yield return (object)new WaitForSeconds(16f / feijiSpeed);
		canMove = true;
		zhadanGameObject.SetActive(true);
		yield return (object)new WaitForSeconds(delay);
		zbAudioMgr.Instance.PlayAudio("audio/combat/kongtouhongzha_hit.ogg");
		canMove = false;
		zhadanGameObject.SetActive(false);
		((Component)boomAnimation).gameObject.SetActive(true);
		TrackEntry val = boomAnimation.AnimationState.SetAnimation(0, "animation", false);
		((Component)this).transform.localScale = new Vector3(damageRange, damageRange, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		ReturnToPool(val.AnimationEnd + 1f / 3f);
	}
}
