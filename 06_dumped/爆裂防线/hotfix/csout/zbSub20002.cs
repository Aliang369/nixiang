using Proto.Enums;
using UnityEngine;

public class zbSub20002 : zbSkillBase
{
	public zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y, ((Component)this).transform.position.z + ((Component)this).transform.position.x / 100f + ((Component)this).transform.position.y / 10f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y + 0.3f;
		boundsCircle2D.radius = attrValueByType / 2f;
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}
}
