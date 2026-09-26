using Proto.Enums;
using UnityEngine;

public class zbSub10704 : zbSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			((Component)this).transform.position = monsterById.GetCurPosition();
		}
		float attrValueByType = GetAttrValueByType(EAttrSkillType.ExplosionRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		ReturnToPool(0.4f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.ExplosionDamage);
	}
}
