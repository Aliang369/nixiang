using Proto.Enums;
using UnityEngine;

public class zbSub20001 : zbSkillBase
{
	public zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType / 2f, attrValueByType / 2f, 1f);
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y, ((Component)this).transform.position.z + ((Component)this).transform.position.x / 100f + ((Component)this).transform.position.y / 10f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y + 0.3f;
		boundsCircle2D.radius = attrValueByType / 2f;
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}
}
