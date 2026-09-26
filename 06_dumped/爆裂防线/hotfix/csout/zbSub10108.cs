using Proto.Enums;
using UnityEngine;

public class zbSub10108 : zbSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		ReturnToPool(0.4f);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage);
	}
}
