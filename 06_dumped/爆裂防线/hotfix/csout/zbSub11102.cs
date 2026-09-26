using Proto.Enums;
using UnityEngine;

public class zbSub11102 : zbSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		}
		ReturnToPool(0.4f);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.ExplosionDamage);
	}
}
