using UnityEngine;

public class zbBuffIndex17 : zbBuffBase
{
	private float currSlowDown;

	public Vector3 startPoint;

	public Vector3 endPoint;

	public float duration;

	private float elapsed;

	private bool moving;

	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecSlowDownPercent(currSlowDown);
			monsterById.moveDownBuff17 = true;
			moving = false;
			elapsed = 0f;
		}
		base.Reset();
		currSlowDown = 0f;
	}

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		currSlowDown = 1f;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.AddSlowDownPercent(currSlowDown);
			elapsed = 0f;
			moving = true;
			monsterById.moveDownBuff17 = false;
		}
	}

	public override void Update()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!moving)
		{
			return;
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			((Component)monsterById.anim.spineAnimation.skeletonAnimation).transform.localPosition = Vector3.Lerp(startPoint, endPoint, num);
			if (num >= 1f)
			{
				moving = false;
				((Component)monsterById.anim.spineAnimation.skeletonAnimation).transform.localPosition = endPoint;
			}
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecSlowDownPercent(currSlowDown);
			monsterById.moveDownBuff17 = true;
			moving = false;
			elapsed = 0f;
		}
		currSlowDown = 0f;
	}

	public zbBuffIndex17()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		startPoint = new Vector3(0f, 0f, 0f);
		endPoint = new Vector3(0f, 0.5f, 0f);
		duration = 1f;
		base._002Ector();
	}
}
