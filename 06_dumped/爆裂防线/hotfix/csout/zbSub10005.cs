using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSub10005 : zbSkillBase
{
	private List<int> m_monstersInRange = new List<int>();

	private SkeletonAnimation skeletonDandao;

	private GameObject gameObjectEmpty;

	[NonSerialized]
	public Bone gunpoint;

	protected override void OnInited()
	{
		base.OnInited();
		skeletonDandao = ((Component)((Component)this).transform.Find("dandao")).GetComponent<SkeletonAnimation>();
		gameObjectEmpty = ((Component)((Component)this).transform.Find("emptyObject")).gameObject;
		gunpoint = ((SkeletonRenderer)skeletonDandao).skeleton.FindBone("shootpoint");
		m_monstersInRange.Clear();
		((MonoBehaviour)this).StartCoroutine(ActionAmimation());
	}

	public IEnumerator ActionAmimation()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			Vector3 val = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(monsterById.GetCurPosition());
			SkeletonExtensions.SetLocalPosition(gunpoint, val);
		}
		else
		{
			Vector3 val2 = ((Component)m_skeletonAnimation).transform.InverseTransformPoint(new Vector3(Rand.Range(-3f, 3f) + combatData.nodeRootPositonX, Rand.Range(-6f, 6f) + combatData.nodeRootPositonY));
			SkeletonExtensions.SetLocalPosition(gunpoint, val2);
		}
		TrackEntry current = ((Rand.Range(0, 2) != 0) ? m_skeletonAnimation.AnimationState.SetAnimation(0, "animation2", false) : m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false));
		yield return (object)new WaitForSeconds(1.5333333f);
		int[][] skillSub = skillSubRecord.skillSub;
		foreach (int[] array in skillSub)
		{
			for (int j = 0; j < array[0]; j++)
			{
				zbSkillSystem.Instance.CreateSubSkill(array[1], hero, this, skillAttrList, gameObjectEmpty.transform.position, Vector2.up);
			}
		}
		yield return (object)new WaitForSeconds(current.AnimationEnd - 0.5f);
		ReturnToPool();
	}
}
