using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub10703 : zbSkillBase
{
	private GameObject emptyDianqiu;

	private GameObject animationDianqiu;

	private float laserWidth = 0.1f;

	private LineRenderer lineRenderer;

	private int ricochetMaxNum = 1;

	private int targetMonsterId;

	private Vector2 targetPosition;

	private List<int> targetIdList;

	private bool continueRun = true;

	private float bulletSpeed = 1f;

	private int lastTargetMonsterId;

	protected override void OnInited()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		emptyDianqiu = zbUtil.FindGameObjectRecursive(((Component)this).transform, "EmptyDianqiu");
		animationDianqiu = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonAnimation");
		lineRenderer = ((Component)this).gameObject.GetComponent<LineRenderer>();
		if ((Object)(object)lineRenderer == (Object)null)
		{
			lineRenderer = ((Component)this).gameObject.GetComponentInChildren<LineRenderer>();
		}
		lineRenderer.startWidth = laserWidth;
		lineRenderer.endWidth = laserWidth;
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, ((Component)this).transform.position);
		lineRenderer.SetPosition(1, ((Component)this).transform.position);
		emptyDianqiu.transform.position = ((Component)this).transform.position;
		ricochetCount = 0;
		ricochetMaxNum = (int)GetAttrValueByType(EAttrSkillType.RicochetCount);
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		targetMonsterId = 1;
		lastTargetMonsterId = 0;
		targetPosition = new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY);
		targetIdList = new List<int>();
		continueRun = true;
		triggerRicochet();
		ReturnToPool(20f);
	}

	public void triggerRicochet()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (ricochetCount >= ricochetMaxNum)
		{
			ReturnToPool(0.1f);
			continueRun = false;
			return;
		}
		continueRun = false;
		for (int i = 0; i < 50; i++)
		{
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if (Object.op_Implicit((Object)(object)randomMonster) && lastTargetMonsterId != randomMonster.GetUUID())
			{
				targetMonsterId = randomMonster.GetUUID();
				lastTargetMonsterId = randomMonster.GetUUID();
				targetPosition = Vector2.op_Implicit(randomMonster.GetCurPosition());
				targetIdList.Add(targetMonsterId);
				continueRun = true;
				break;
			}
		}
		if (!continueRun)
		{
			ReturnToPool();
		}
		ricochetCount++;
		lineRenderer.positionCount = ricochetCount + 1;
		lineRenderer.SetPosition(ricochetCount, lineRenderer.GetPosition(ricochetCount - 1));
	}

	protected override void Update()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!continueRun)
		{
			return;
		}
		emptyDianqiu.transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(emptyDianqiu.transform.position), targetPosition, bulletSpeed * Time.deltaTime));
		Vector3 position = emptyDianqiu.transform.position;
		lineRenderer.SetPosition(ricochetCount, new Vector3(position.x, position.y, 0f));
		animationDianqiu.transform.position = position;
		if (zbUtil.Reached(Vector2.op_Implicit(emptyDianqiu.transform.position), targetPosition))
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterId);
			if ((Object)(object)monsterById != (Object)null)
			{
				CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
			}
			triggerRicochet();
		}
	}
}
