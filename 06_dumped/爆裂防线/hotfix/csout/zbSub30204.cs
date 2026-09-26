using System;
using Proto.Enums;
using UnityEngine;

public class zbSub30204 : zbSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 1f;

	private int selfSortNumber = 1;

	private zbBoundsCircle2D boundsCircle2D;

	private bool createOtherFlag;

	private bool createOtherLast;

	protected override void OnInited()
	{
		base.OnInited();
		createOtherFlag = false;
		createOtherLast = false;
		selfSortNumber = 1;
		bulletSpeed = GetAttrValueByType(EAttrSkillType.BulletSpeed);
		ignoreMonsters.Clear();
		ignoreMonsters.Add(mainMonsterUUID);
		ReturnToPool(lifetime);
	}

	private void SetSelfNumber(int number)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		selfSortNumber = number;
		if (selfSortNumber == 1)
		{
			SetDirection(Vector2.up);
		}
		else if (selfSortNumber == 2)
		{
			SetDirection(Vector2.down);
		}
		else if (selfSortNumber == 3)
		{
			SetDirection(Vector2.left);
		}
		else
		{
			SetDirection(Vector2.right);
		}
		((Component)this).transform.Translate(m_direction * 0.5f, (Space)0);
	}

	private void SetSelfCreate()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (selfSortNumber == 1)
		{
			for (int i = 2; i < 5; i++)
			{
				((zbSub30204)zbSkillSystem.Instance.CreateSubSkill(30204, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, mainMonsterUUID)).SetSelfNumber(i);
			}
		}
	}

	protected override void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!createOtherLast)
		{
			createOtherLast = true;
			return;
		}
		if (!createOtherFlag)
		{
			createOtherFlag = true;
			if (selfSortNumber == 1)
			{
				SetSelfCreate();
			}
		}
		skillTF.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		boundsCircle2D.center.x = skillTF.position.x;
		boundsCircle2D.center.y = skillTF.position.y;
		boundsCircle2D.radius = 0.15f;
		UpdateMonsterCollision(boundsCircle2D, ignoreMonsters, EAttrSkillType.Damage, stopAtFirst: true);
	}
}
