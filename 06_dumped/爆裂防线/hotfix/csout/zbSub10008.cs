using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSub10008 : zbSkillBase
{
	private SkeletonAnimation skeletonDandao;

	protected override void OnInited()
	{
		base.OnInited();
		skeletonDandao = ((Component)((Component)this).transform.Find("dandao")).GetComponent<SkeletonAnimation>();
		((MonoBehaviour)this).StartCoroutine(ActionAmimation());
	}

	public IEnumerator ActionAmimation()
	{
		TrackEntry current = skeletonDandao.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(1.3333334f);
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		int[][] skillSub;
		if ((Object)(object)monsterById != (Object)null)
		{
			skillSub = skillSubRecord.skillSub;
			foreach (int[] array in skillSub)
			{
				zbSkillSystem.Instance.CreateSubSkill(array[1], hero, null, hero.heroAttr.skillAttrList, ((Component)monsterById).transform.position, Vector2.up, mainMonsterUUID);
			}
		}
		combatData.monsters.GetRandomMonsters(5, monsterRandomList, mainMonsterUUID);
		skillSub = skillSubRecord.skillSub;
		foreach (int[] array2 in skillSub)
		{
			for (int j = 0; j < array2[0] - 1; j++)
			{
				int uuid = ((monsterRandomList.Count > j) ? monsterRandomList[j].GetUUID() : 0);
				zbMonster monsterById2 = combatData.monsters.GetMonsterById(uuid);
				if ((Object)(object)monsterById2 != (Object)null)
				{
					zbSkillSystem.Instance.CreateSubSkill(array2[1], hero, null, hero.heroAttr.skillAttrList, ((Component)monsterById2).transform.position, Vector2.up, uuid);
				}
				else
				{
					zbSkillSystem.Instance.CreateSubSkill(array2[1], hero, null, hero.heroAttr.skillAttrList, Vector3.zero, Vector2.up);
				}
			}
		}
		yield return (object)new WaitForSeconds(current.AnimationEnd - 0.5f);
		ReturnToPool();
	}
}
