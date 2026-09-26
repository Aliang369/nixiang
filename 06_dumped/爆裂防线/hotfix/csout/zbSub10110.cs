using System;
using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbSub10110 : zbSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 1f;

	private SkeletonAnimation skeletonDaoguang;

	private GameObject gameObjectDaoguang_2;

	private bool canMove;

	protected override void OnInited()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		skeletonDaoguang = ((Component)((Component)this).transform.Find("daoguang")).GetComponent<SkeletonAnimation>();
		gameObjectDaoguang_2 = ((Component)((Component)this).transform.Find("daoguang_2")).gameObject;
		gameObjectDaoguang_2.SetActive(false);
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.defense.GetAlertPositionY() + combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		gameObjectDaoguang_2.transform.localPosition = new Vector3(0f, 0f, 0f);
		canMove = false;
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		ignoreMonsters.Clear();
		((MonoBehaviour)this).StartCoroutine(ActionAmimation());
		ReturnToPool(10f);
	}

	public IEnumerator ActionAmimation()
	{
		skeletonDaoguang.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(1.0333333f);
		zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
		canMove = true;
		gameObjectDaoguang_2.SetActive(true);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null && !ignoreMonsters.Contains(component.GetUUID()))
		{
			CalcMonsterDamage(EAttrSkillType.Damage, component);
			ignoreMonsters.Add(component.GetUUID());
		}
	}

	protected override void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (canMove)
		{
			gameObjectDaoguang_2.transform.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		}
	}
}
