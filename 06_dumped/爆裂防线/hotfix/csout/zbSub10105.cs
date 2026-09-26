using Proto.Enums;
using UnityEngine;

public class zbSub10105 : zbSkillBase
{
	public zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}
}
