using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class zbMonsterHitEffect : MonoBehaviour
{
	private zbCombatData combatData;

	private SkeletonAnimation skeletonAnimation;

	private int monsterUUID;

	private float m_returnPoolTime;

	private float m_returnPoolTimeAccumTime;

	private const float FLY_START_DELAY = 0.3f;

	private const float MIN_FLY_DURATION = 0.35f;

	private const float BASE_SPEED = 6f;

	private Vector3 m_initialScale;

	private void Reset()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		combatData = null;
		m_returnPoolTime = float.MaxValue;
		m_returnPoolTimeAccumTime = 0f;
		((Component)this).transform.localScale = Vector3.one;
	}

	public void InitData(zbCombatData _combatData, int _monsterUUID)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		}
		combatData = _combatData;
		((Component)this).gameObject.transform.SetParent(combatData.nodeRootMonsterSkillEffect);
		monsterUUID = _monsterUUID;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			SetThisPosition(monsterById);
			m_initialScale = ((Component)this).transform.localScale;
			Vector3 position = ((Component)this).transform.position;
			zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
			Rect worldBound = zbCombatWnd2.m_killCount.worldBound;
			Vector3 val = zbUtil.UIToolkitToWorldSpace(((Rect)(ref worldBound)).position, zbCameraHelper.Instance.mainCamera, zbCombatWnd2.m_killCount);
			((MonoBehaviour)this).StartCoroutine(DelayThenFly(position, new Vector3(val.x + 0.2f, val.y - 0.25f, val.z)));
		}
		else
		{
			ReturnToPool(0.01f);
		}
	}

	public void SetThisPosition(zbMonster targetMonster)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Vector3 curPosition = targetMonster.GetCurPosition();
		float num = Rand.Range(0.3f, 0.8f);
		float num2 = Rand.Range(0f, (float)Math.PI * 2f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Cos(num2) * num, Mathf.Sin(num2) * num, 0f);
		((Component)this).transform.position = new Vector3(curPosition.x, curPosition.y, -6f) + val;
		((Component)this).transform.localScale = ((Component)targetMonster).transform.localScale;
	}

	private IEnumerator PopOutAnim()
	{
		Vector2 start = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 end = start + Rand.insideUnitCircle * 0.3f;
		float t = 0f;
		float duration = 0.15f;
		while (t < duration)
		{
			t += Time.deltaTime;
			float num = Mathf.Sin(t / duration * (float)Math.PI);
			((Component)this).transform.position = Vector3.Lerp(Vector2.op_Implicit(start), Vector2.op_Implicit(end), num);
			yield return null;
		}
	}

	private IEnumerator DelayThenFly(Vector3 startPos, Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		yield return PopOutAnim();
		yield return null;
		if ((Object)(object)skeletonAnimation != (Object)null && skeletonAnimation.AnimationState != null)
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
		}
		yield return (object)new WaitForSeconds(0.3f);
		yield return FlyBezierToTarget(((Component)this).transform.position, targetPos, 6f);
	}

	private IEnumerator FlyBezierToTarget(Vector3 startPos, Vector3 targetPos, float baseSpeed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float fixedZ = ((Component)this).transform.position.z;
		startPos = ((Component)this).transform.position;
		float num = Vector3.Distance(startPos, targetPos);
		float duration = Mathf.Max(num / baseSpeed, 0.35f);
		Vector3 val = targetPos - startPos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(0f - normalized.y, normalized.x, 0f);
		float num2 = Rand.Range(2f, 3f) * num / 10f;
		float num3 = Rand.Range(-1f, 1f);
		float num4 = 1f;
		if (startPos.x > 0f && startPos.x < 3.6f)
		{
			num4 = -1f;
		}
		else if (startPos.x < 0f && startPos.x > -3.6f)
		{
			num4 = 1f;
		}
		float num5 = Rand.Range(0.15f, 0.25f);
		Vector3 controlPos = startPos + (targetPos - startPos) * num5;
		controlPos += val2 * num2 * num4;
		controlPos.x += num3;
		controlPos.z = fixedZ;
		float elapsed = 0f;
		float rotateSpeed = 180f;
		float dir = ((Rand.value > 0.5f) ? 1f : (-1f));
		float totalRotation = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float t2 = EaseInQuad(t);
			Vector3 position = CalculateBezierPoint(t2, startPos, controlPos, targetPos);
			position.z = fixedZ;
			((Component)this).transform.position = position;
			((Component)this).transform.localScale = m_initialScale;
			if ((Object)(object)skeletonAnimation != (Object)null)
			{
				totalRotation += rotateSpeed * Time.deltaTime * dir;
				((Component)skeletonAnimation).transform.localEulerAngles = new Vector3(0f, 0f, totalRotation);
			}
			yield return null;
		}
		Vector3 position2 = default(Vector3);
		((Vector3)(ref position2))._002Ector(targetPos.x, targetPos.y, fixedZ);
		((Component)this).transform.position = position2;
		((Component)this).transform.localScale = m_initialScale;
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			((Component)skeletonAnimation).transform.localEulerAngles = Vector3.zero;
		}
		(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).PlayIconScaleAnim();
		ReturnToPool(0.01f);
	}

	private float EaseInQuad(float t)
	{
		return t * t;
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		return num * num * p0 + 2f * num * t * p1 + t * t * p2;
	}

	public void ReturnToPool(float delayTime = 0f)
	{
		if (((Component)this).gameObject.activeSelf)
		{
			m_returnPoolTime = delayTime;
			m_returnPoolTimeAccumTime = 0f;
			if (delayTime == 0f)
			{
				ReturnToPoolImmediate();
			}
		}
	}

	private void ReturnToPoolImmediate()
	{
		if (((Component)this).gameObject.activeSelf && combatData != null && combatData.monsterHitEffectPoolMgr != null)
		{
			combatData.monsterHitEffectPoolMgr.ReturnMonsterHitEffect(this);
		}
	}

	private void Update()
	{
		m_returnPoolTimeAccumTime += Time.deltaTime;
		if (m_returnPoolTimeAccumTime >= m_returnPoolTime)
		{
			ReturnToPoolImmediate();
		}
	}
}
