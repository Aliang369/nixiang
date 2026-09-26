using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zbMonsterSkillIndex30051 : zbMonsterSkillBase
{
	public stMonsterSkillIndex30051 data = new stMonsterSkillIndex30051();

	private int monsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.everyTime = monsterSkillRecord.param[0];
		data.distance = monsterSkillRecord.param[1] / 100f;
		data.durationTime = monsterSkillRecord.param[2];
		monsterUUID = monster.GetUUID();
	}

	public override float GetDiameter()
	{
		return data.distance / 3f * 2f;
	}

	public override void Update()
	{
		if (IsBuffStop())
		{
			return;
		}
		data.everyTimeKeep += Time.deltaTime;
		if (data.everyTimeKeep >= data.everyTime)
		{
			data.everyTimeKeep = 0f;
			if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
			{
				CreateEffectSpine();
				monster.canMoveOrAttack = false;
				data.durationTimeKeep = data.durationTime;
				((MonoBehaviour)monster).StartCoroutine(ActionCoroutine());
				((MonoBehaviour)monster).StartCoroutine(CleanupSequence());
			}
		}
	}

	public IEnumerator ActionCoroutine()
	{
		PlayTackAnimation(1, flag: true);
		while (true)
		{
			data.durationTimeKeep -= Time.deltaTime;
			if (data.durationTimeKeep <= 0f)
			{
				break;
			}
			List<zbSkillBase> list = new List<zbSkillBase>();
			foreach (KeyValuePair<string, zbSkillPool> item in monster.combatData.skillPoolMgr.m_pool)
			{
				LinkedListNode<zbSkillBase> linkedListNode = item.Value.m_activeSkills.First;
				while (linkedListNode != null)
				{
					LinkedListNode<zbSkillBase>? next = linkedListNode.Next;
					if (((linkedListNode.Value.isMainSkill && linkedListNode.Value.skillRecord.attackType != 5) || (linkedListNode.Value.isSubSkill && linkedListNode.Value.skillSubRecord.id < 20000 && linkedListNode.Value.skillSubRecord.attackType != 5)) && zbUtil.GetVector2Distance(((Component)linkedListNode.Value).transform.position, ((Component)monster).transform.position) <= data.distance)
					{
						list.Add(linkedListNode.Value);
					}
					linkedListNode = next;
				}
			}
			foreach (zbSkillBase item2 in list)
			{
				item2.ReturnToPool();
			}
			zbCombatSystem.Instance.buffSystem.RemoveBuffInDistance(((Component)monster).transform.position, data.distance);
			yield return null;
		}
	}

	public IEnumerator CleanupSequence()
	{
		yield return (object)new WaitForSeconds(data.durationTime);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
			FreeEffectSpine();
		}
	}

	public override void ClearList()
	{
	}
}
