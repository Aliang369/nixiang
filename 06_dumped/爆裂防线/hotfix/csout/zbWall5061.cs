using Proto.Enums;
using Spine;
using UnityEngine;

public class zbWall5061 : zbWallSkillBase
{
	private GameObject goRiseEffect_1;

	private BoxCollider2D boxCollider;

	public Vector2 boxColliderOffset;

	public Vector2 boxColliderSize;

	private float m_lastRiseY;

	public stWallSkillIndex5061 wallSkillIndex5061Data;

	protected override void OnInited()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		goRiseEffect_1 = ((Component)((Component)this).transform.Find("emptyObject")).gameObject;
		boxCollider = ((Component)((Component)this).transform).GetComponent<BoxCollider2D>();
		((Collider2D)boxCollider).offset = boxColliderOffset;
		boxCollider.size = boxColliderSize;
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		if (val != null)
		{
			ReturnToPool(val.AnimationEnd);
		}
		else
		{
			ReturnToPool(2.3333333f);
		}
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		base.SetWallAtr(_damage, _wallSkillRecord);
		skillAttrList[EAttrSkillType.Knockback] = wallSkillIndex5061Data.knockback;
	}

	protected override void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		float y = goRiseEffect_1.transform.position.y;
		if (y != m_lastRiseY)
		{
			m_lastRiseY = y;
			boxColliderSize.y = y + 8f;
			boxColliderOffset.y = (y + 8f) / 2f - 8f;
			((Collider2D)boxCollider).offset = boxColliderOffset;
			boxCollider.size = boxColliderSize;
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			ShootFormulaMonster(component);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (targetMonster.monsterRecord.moveType != (int)wallSkillIndex5061Data.towMonsterType)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, targetMonster, base.damage, wallSkillRecord.atkType);
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.up, targetMonster.GetUUID());
		}
	}

	public zbWall5061()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		boxColliderOffset = new Vector2(0f, -8f);
		boxColliderSize = new Vector2(10f, 0.1f);
		m_lastRiseY = float.MinValue;
		base._002Ector();
	}
}
