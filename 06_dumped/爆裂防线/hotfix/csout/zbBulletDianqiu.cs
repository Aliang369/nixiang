using Proto.Enums;
using UnityEngine;

public class zbBulletDianqiu : zbSkillBase
{
	private GameObject emptyDianqiu;

	private GameObject animationDianqiu;

	private float laserWidth = 0.2f;

	private LineRenderer lineRenderer;

	private int ricochetMaxNum = 1;

	private int targetMonsterId;

	private Vector2 targetPosition;

	private bool continueRun = true;

	private float bulletSpeed = 1f;

	private zbBulletDianqiu lastBulletDianqiu;

	private bool ReturnToPooled;

	private zbOBB2D boundsOBB2D;

	private bool firstDisable = true;

	private bool isSplited;

	private float gemMod2121Value;

	private float gemMod2124Value;

	protected override void OnInited()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		emptyDianqiu = zbUtil.FindGameObjectRecursive(((Component)this).transform, "EmptyDianqiu");
		animationDianqiu = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonAnimation");
		animationDianqiu.SetActive(true);
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
		((Renderer)lineRenderer).material = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/" + hero.skillRecord.bulletSpineName + ".mat");
		SetBoxCollider2D(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(((Component)this).transform.position));
		ricochetCount = 1;
		ricochetMaxNum = (int)GetAttrValueByType(EAttrSkillType.RicochetCount);
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		isSplited = false;
		targetMonsterId = 0;
		targetPosition = new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY);
		continueRun = true;
		ReturnToPooled = false;
		firstDisable = true;
		gemMod2121Value = hero.team.ExtraProperties[ETeamExtra.GemMod2121];
		gemMod2124Value = hero.team.ExtraProperties[ETeamExtra.GemMod2124];
		ReturnToPool(3f);
	}

	public void SetBoxCollider2D(Vector2 start, Vector2 end)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		boundsOBB2D.SetPram(start, end, laserWidth);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ProdReturnToPool();
	}

	public void ProdReturnToPool()
	{
		ReturnToPooled = true;
		if ((Object)(object)lastBulletDianqiu != (Object)null && !lastBulletDianqiu.ReturnToPooled)
		{
			lastBulletDianqiu.ReturnToPooled = true;
			lastBulletDianqiu.ProdReturnToPool();
		}
		if (firstDisable && (gemMod2121Value > 0f || gemMod2124Value > 0f))
		{
			lineRenderer.startWidth = laserWidth * 6f;
			lineRenderer.endWidth = laserWidth * 6f;
			CollisionCooldown();
		}
		firstDisable = false;
		ReturnToPool(0.5f);
	}

	public void triggerRicochet()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (ricochetCount >= ricochetMaxNum)
		{
			ProdReturnToPool();
			return;
		}
		int num = 1;
		if (zbUtil.Roll0to1(hero.team.ExtraProperties[ETeamExtra.DianqiuMod2122Roll]) && !isSplited)
		{
			num += (int)hero.team.ExtraProperties[ETeamExtra.DianqiuMod2122Cont];
		}
		int num2 = 0;
		combatData.monsters.GetRandomMonsterList(num, monsterRandomList, targetMonsterId);
		foreach (zbMonster monsterRandom in monsterRandomList)
		{
			((zbBulletDianqiu)zbSkillSystem.Instance.CreateSkill("prefab/combat/hero/dianqiu/bulletDianqiu.prefab", hero, emptyDianqiu.transform.position, Vector2.up, monsterRandom.GetUUID())).setDianqiuMonsterId(monsterRandom.GetUUID(), monsterRandom.GetCurPosition(), this, ricochetCount + 1, num2 == 1);
			num2++;
		}
		if (monsterRandomList.Count <= 0)
		{
			ProdReturnToPool();
		}
	}

	public void setDianqiuMonsterId(int _targetMonsterId, Vector3 _targetPosition, zbBulletDianqiu _lastBulletDianqiu, int _ricochetCount = 1, bool _isSplited = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		targetMonsterId = _targetMonsterId;
		targetPosition = Vector2.op_Implicit(_targetPosition);
		ricochetCount = _ricochetCount;
		lastBulletDianqiu = _lastBulletDianqiu;
		for (int i = 0; i <= 36; i++)
		{
			EAttrSkillType eAttrSkillType = (EAttrSkillType)i;
			if (IsDamageType(eAttrSkillType) && skillAttrList.TryGetValue(eAttrSkillType, out var value))
			{
				skillAttrList[eAttrSkillType] = value * Mathf.Pow(0.9f, (float)ricochetCount);
			}
		}
		isSplited = _isSplited;
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed) * (1f + 0.2f * (float)ricochetCount);
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
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!continueRun)
		{
			return;
		}
		emptyDianqiu.transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(emptyDianqiu.transform.position), targetPosition, bulletSpeed * Time.deltaTime));
		Vector3 position = emptyDianqiu.transform.position;
		lineRenderer.SetPosition(1, new Vector3(position.x, position.y, 0f));
		animationDianqiu.transform.position = position;
		if (zbUtil.Reached(Vector2.op_Implicit(emptyDianqiu.transform.position), targetPosition))
		{
			continueRun = false;
			zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterId);
			if ((Object)(object)monsterById != (Object)null)
			{
				CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
			}
			triggerRicochet();
			animationDianqiu.SetActive(false);
		}
		SetBoxCollider2D(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(emptyDianqiu.transform.position));
	}

	private void CollisionCooldown()
	{
		combatData.monsters.monsterGrid.QueryList(in boundsOBB2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ShootFormulaMonster(overlapResult);
		}
	}

	private void ShootFormulaMonster(zbMonster targetMonster)
	{
		if ((Object)(object)targetMonster != (Object)null)
		{
			if (hero.team.ExtraProperties[ETeamExtra.GemMod2121] > 0f)
			{
				CalcMonsterDamage(EAttrSkillType.Damage, targetMonster, hero.team.ExtraProperties[ETeamExtra.GemMod2121] * (float)(int)hero.team.teamAttack, param: true);
			}
			if (hero.team.ExtraProperties[ETeamExtra.GemMod2124] > 0f)
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, targetMonster.GetUUID(), 3, hero.team.ExtraProperties[ETeamExtra.GemMod2124]);
			}
		}
	}
}
