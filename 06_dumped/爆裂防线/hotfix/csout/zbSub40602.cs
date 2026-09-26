using System;
using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub40602 : zbSkillBase
{
	private GameObject feijiGameObject;

	private GameObject zhadanGameObject;

	private SkeletonAnimation boomAnimation;

	private SkeletonAnimation xiaoxingAnimation;

	private float feijiSpeed = 12f;

	private float zhadanSpeed = 20f;

	private float hitTime = 1f;

	private bool canMove = true;

	private bool canFight = true;

	private zbBoundsCircle2D boundsCircle2D;

	private float damageRange = 1f;

	private Vector3 targetP;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		canFight = false;
		canMove = false;
		feijiGameObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonFeiji");
		feijiGameObject.SetActive(false);
		zhadanGameObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonZhadan");
		zhadanGameObject.SetActive(false);
		zbAudioMgr.Instance.PlayAudio("audio/combat/kongtouhongzha_fire.ogg");
		boomAnimation = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonBoom").GetComponent<SkeletonAnimation>();
		((Component)boomAnimation).gameObject.SetActive(false);
		xiaoxingAnimation = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonXiaoxing").GetComponent<SkeletonAnimation>();
		((Component)xiaoxingAnimation).gameObject.SetActive(false);
		zhadanGameObject.transform.position = new Vector3(((Component)this).transform.position.x, targetP.y + 14f, targetP.z);
		hitTime = 14f / zhadanSpeed;
		targetP = ((Component)this).transform.position;
		m_damageIntervalPre = 1f / 3f;
		m_damageInterval = 1f / 3f;
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
		if (canFight)
		{
			m_damageIntervalPre += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
			}
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
		canFight = true;
		zhadanGameObject.SetActive(false);
		((Component)boomAnimation).gameObject.SetActive(true);
		((Component)xiaoxingAnimation).gameObject.SetActive(true);
		boomAnimation.AnimationState.SetAnimation(0, "animation", false);
		xiaoxingAnimation.AnimationState.SetAnimation(0, "animation", false);
		((Component)xiaoxingAnimation).transform.localScale = new Vector3(1f / damageRange, 1f / damageRange, 1f);
		((Component)this).transform.localScale = new Vector3(damageRange, damageRange, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = ((Component)this).transform.localScale.x / 2f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
		ReturnToPool(2f);
	}
}
