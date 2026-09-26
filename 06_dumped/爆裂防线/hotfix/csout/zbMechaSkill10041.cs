using Proto.Enums;
using UnityEngine;

public class zbMechaSkill10041 : zbMechaSkillBase
{
	private BoxCollider2D boxCollider;

	private Vector2 boxColliderOffset;

	private Vector2 boxColliderSize;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		boxCollider = ((Component)this).GetComponent<BoxCollider2D>();
		ReturnToPool(lifetime);
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		boxColliderSize.y = mechaSkillRecord.param[1] / 100f;
		boxColliderOffset.y = boxColliderSize.y / 2f;
		((Collider2D)boxCollider).offset = boxColliderOffset;
		boxCollider.size = boxColliderSize;
		skillAttrList[EAttrSkillType.Knockback] = _mechaSkillRecord.param[0];
		ReturnToPool(1f / 3f);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.up, component.GetUUID());
		}
	}

	public zbMechaSkill10041()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		boxColliderOffset = new Vector2(0f, 0f);
		boxColliderSize = new Vector2(8f, 1f);
		base._002Ector();
	}
}
