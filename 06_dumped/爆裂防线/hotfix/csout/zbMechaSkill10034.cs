using System.Collections;
using Proto.Enums;
using UnityEngine;

public class zbMechaSkill10034 : zbMechaSkillBase
{
	private zbBounds2D bounds2D;

	private int damageIndex;

	private float y;

	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		skillAttrList[EAttrSkillType.Knockback] = _mechaSkillRecord.param[0];
		y = combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY() + 1f;
		bounds2D.center.x = 0f;
		bounds2D.halfSize.x = 4f;
		bounds2D.halfSize.y = (8.4f - y) / 10f;
		((MonoBehaviour)this).StartCoroutine(PlayAudio());
	}

	public IEnumerator PlayAudio()
	{
		damageIndex = 0;
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		bounds2D.center.y = ((Component)this).transform.position.y + bounds2D.halfSize.y;
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(1f / 3f);
		damageIndex = 1;
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		bounds2D.center.y = ((Component)this).transform.position.y + bounds2D.halfSize.y * 3f;
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(1f / 3f);
		damageIndex = 2;
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		bounds2D.center.y = ((Component)this).transform.position.y + bounds2D.halfSize.y * 5f;
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(1f / 3f);
		damageIndex = 3;
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		bounds2D.center.y = ((Component)this).transform.position.y + bounds2D.halfSize.y * 7f;
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(1f / 3f);
		damageIndex = 4;
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		bounds2D.center.y = ((Component)this).transform.position.y + bounds2D.halfSize.y * 9f;
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(1.3333334f);
		damageIndex = 5;
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		bounds2D.halfSize.y = 8f;
		bounds2D.center.y = 0f;
		UpdateWallCollision();
		ReturnToPool();
	}

	protected void UpdateWallCollision()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsters.monsterGrid.QueryList(in bounds2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, base.damage * mechaSkillRecord.param[damageIndex], mechaSkillRecord.atkType);
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, ((Component)overlapResult).transform.position, Vector2.up, overlapResult.GetUUID());
		}
	}
}
