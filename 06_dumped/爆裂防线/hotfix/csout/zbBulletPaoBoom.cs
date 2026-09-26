using Proto.Enums;
using Spine;
using UnityEngine;

public class zbBulletPaoBoom : zbSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		m_sortType = ESortLayerType.BurnLayer;
		float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "3", false);
		ReturnToPool(val.AnimationEnd);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.ExplosionDamage);
	}
}
