using System;
using System.Collections;
using UnityEngine;

public class zbMechaSkill10053 : zbMechaSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	[NonSerialized]
	public GameObject goRiseEffect_1;

	private zbMechaNengliang01 mechaNengliang01;

	protected override void OnInited()
	{
		base.OnInited();
		mechaNengliang01 = (zbMechaNengliang01)hero;
		boundsCircle2D.center.x = 0f;
		boundsCircle2D.center.y = 0f;
		ignoreMonsters.Clear();
		((MonoBehaviour)this).StartCoroutine(PlayAudio());
	}

	public IEnumerator PlayAudio()
	{
		yield return (object)new WaitForSeconds(1.8333334f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision(0);
		yield return (object)new WaitForSeconds(0.46666658f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision(1);
		yield return (object)new WaitForSeconds(0.4333334f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision(2);
		yield return (object)new WaitForSeconds(0.4666667f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision(3);
		yield return (object)new WaitForSeconds(0.4666667f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision(4);
		yield return (object)new WaitForSeconds(0.99999976f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision(5);
	}

	protected void UpdateWallCollision(int index)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		boundsCircle2D.radius = mechaNengliang01.goRiseEffect_3.transform.position.x;
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, base.damage * mechaSkillRecord.param[index], mechaSkillRecord.atkType);
		}
	}
}
